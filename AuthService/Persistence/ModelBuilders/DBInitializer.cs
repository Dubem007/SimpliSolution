using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AuthService.Persistence.ApplicationContext;

namespace AuthService.Persistence.ModelBuilders
{
    public static class DBInitializer
    {
        public static async Task SeedRoleData(this IHost host)
        {
            var serviceProvider = host.Services.CreateScope().ServiceProvider;
            var context = serviceProvider.GetRequiredService<AuthDbContext>();
            var roles = UserRoleData.GetRoles();

            if (!context.UserRoles.Any())
            {
                await context.UserRoles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }
        }

    }
}
