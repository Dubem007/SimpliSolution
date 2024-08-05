using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Persistence.ModelBuilders
{
    public static class UserRoleData
    {
        public static List<UserRole> GetRoles()
        {
            return new List<UserRole>
            {
                new UserRole
                {
                    RoleName = UserRolesEnum.Administrator.ToString(),
                    RoleDescription = "Manages user access",
                    Name = UserRolesEnum.Administrator.ToString(),
                    NormalizedName = UserRolesEnum.Administrator.ToString().ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                },
                new UserRole
                {
                    RoleName =UserRolesEnum.Regular.ToString(),
                    RoleDescription = "Regular access",
                    Name = UserRolesEnum.Regular.ToString(),
                    NormalizedName = UserRolesEnum.Regular.ToString().ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                },
            };
        }
    }
}
