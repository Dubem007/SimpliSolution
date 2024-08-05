using AuthService.Domain.Entities;
using AuthService.Persistence.ModelBuilders;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.ApplicationContext
{
    public class AuthDbContext: DbContext
    {

        public AuthDbContext(DbContextOptions<AuthDbContext> options): base(options){}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
            modelBuilder.HasDefaultSchema("Simplified_UI");
            modelBuilder.AppModelBuilder();
        }
        //==========
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserProfileRole> UserProfileRoles { get; set; }
       

    }

}
