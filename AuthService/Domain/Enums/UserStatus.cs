using System.ComponentModel;

namespace AuthService.Domain.Enums
{
    public enum UserStatus
    {
        [Description("Locked")]
        Locked = 1,
        [Description("Approved")]
        Approved = 2,
        [Description("Disabled")]
        Disabled = 3,
        [Description("Pending")]
        Pending = 3,
    }
}
