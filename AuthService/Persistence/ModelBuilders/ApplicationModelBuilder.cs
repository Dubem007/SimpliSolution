using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.ModelBuilders
{
    public static class ApplicationModelBuilder
    {
        public static ModelBuilder AppModelBuilder(this ModelBuilder model)
        {
            model.Entity<User>(prop =>
            {
                prop.HasIndex(u => new { u.Id }, name: $"ix_nonclustered_UserId").IsUnique(true);
            });
            model.Entity<UserRole>(prop =>
            {
                prop.HasIndex(u => new { u.Id }, name: $"ix_nonclustered_UserRoleId").IsUnique(true);
            });

            model.Entity<UserProfileRole>(prop =>
            {
                prop.HasIndex(u => new { u.Id }, name: $"ix_RoleId_UserId").IsUnique(true);
                prop.HasIndex(u => new { u.UserId }, name: $"ix_nonclustered_UserId").IsUnique(true);
                prop.HasIndex(u => new { u.RoleId }, name: $"ix_nonclustered_RoleId").IsUnique(true);
            });
           
            return model;
        }
    }
}
