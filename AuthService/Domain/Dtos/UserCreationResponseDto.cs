using AuthService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Dtos
{
    public class UserCreationResponseDto
    {
        public Guid UserId { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; } = false;
        public List<RolesResponse> Roles { get; set; }
       
    }

    public class UserCreationRequestDto
    {
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; } = false;
        public List<Guid> RoleIds { get; set; }

    }

    public class RolesToUserCreationDto
    {
        [Required]
        public Guid UserProfileId { get; set; }
        public List<Guid> UserRoleIds { get; set; }
    }

    public class RolesToUserUpdateDto
    {
        [Required]
        public Guid UserProfileId { get; set; }
        public List<Guid> UserRoleIds { get; set; }
    }

    public class RolesResponse
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool IsUserRole { get; set; }
    }
}
