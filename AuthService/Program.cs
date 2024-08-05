using AuthService.Extensions;
using AuthService.Persistence.ModelBuilders;
using Common.Services.Caching;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using AuthService.Persistence.ApplicationContext;
using AuthService.AppCore.QueueServices;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.AddCachingService();

#region custom services and DI
builder.Services.AddHttpContextAccessor();

//var appConfig = new ConfigurationBuilder()
//                .AddJsonFile("appsettings.json").Build();

string dbConstring = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("Default") ?? string.Empty, EnvironmentVariableTarget.Process) ?? builder.Configuration.GetValue<string>("ConnectionStrings:Default");
#region Serilog configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json").Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Console(formatter: new Serilog.Formatting.Json.JsonFormatter(), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .CreateLogger();
#endregion
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureAutoMapper();
builder.Services.CustomDependencyInjection(builder.Configuration);


builder.Services.AddMediatR(Assembly.GetExecutingAssembly());


builder.Services.AddApiVersioning(options =>
{
    // reporting api versions will return the headers
    // "api-supported-versions" and "api-deprecated-versions"
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(
    options =>
    {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";

        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
        // can also be used to control the format of the API version in route templates
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.OperationFilter<SwaggerDefaultValues>();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },

            },
            new List<string>()
        }
    });
});

builder.Services.AddSession((sessionOptions) =>
{
    sessionOptions.Cookie.Name = "onasc.cookie";
    sessionOptions.IdleTimeout = TimeSpan.FromMinutes(60 * 24);
});

builder.Services.AddHostedService<QueueServices>();
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(dbConstring, options => options.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds))
    .UseLoggerFactory(LoggerFactory.Create(buildr =>
    {
        if (builder.Environment.IsDevelopment())
        {
            buildr.AddDebug();
        }
    }))
    );


builder.Host.UseSerilog();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader().WithMethods("*");
    });
});
#endregion

var app = builder.Build();
try { app.SeedRoleData().Wait(); } catch (Exception ex) { Log.Error(ex.Message, ex); }


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimplifiedUI AuthSvc  v1");
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();


app.UseSession(new SessionOptions
{
    IdleTimeout = TimeSpan.FromMinutes(60 * 24),
    Cookie = new CookieBuilder
    {
        Name = "SimpkifiedUI.Session"
    },
});

app.MapControllers();

app.Run();
