using AuthService.AppCore.Interfaces;
using AuthService.AppCore.Mappers;
using AuthService.AppCore.QueueServices;
using AuthService.AppCore.Repositories;
using AuthService.Domain.Entities;
using AuthService.Persistence.ApplicationContext;
using AutoMapper;
using Common.Services.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Common.MessageQueue.Interfaces;
using Common.MessageQueue.Services;
using System.Text;

namespace AuthService.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var jwtUserSecret = jwtSettings.GetSection("Secret").Value ?? string.Empty;

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.GetSection("ValidIssuer").Value,
                ValidAudience = jwtSettings.GetSection("ValidAudience").Value,
                IssuerSigningKey = new
                    SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtUserSecret))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context => { return Task.CompletedTask; }
            };
        });
    }

    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        var configMap = new MapperConfiguration(cfg => { cfg.AddProfile(new AutoMapperProfile()); });
        var mapper = configMap.CreateMapper();
        services.AddSingleton(mapper);
    }

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        var builder = services.AddIdentity<User, UserRole>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 8;
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();
    }

    public static IServiceCollection CustomDependencyInjection(this IServiceCollection services,
        IConfiguration configuration)
    {
        // services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IMessageQueueService, MessageQueueService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        
        return services;
    }
}