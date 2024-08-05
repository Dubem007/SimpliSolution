namespace AuthService.Domain.Dtos
{
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginResponse
    {
        public string AccessToken { get; set; }
        public DateTime? ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public List<RolesResponse> Roles { get; set; }
        public string UserStatus { get; set; }

    }

    public class ChangePasswordDto
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Username { get; set; }
        public string ResetCode { get; set; }
        public string NewPassword { get; set; }
        public string Module { get; set; }
    }

    public class UnclockAccountDto
    {
        public string Username { get; set; }
        public string ResetCode { get; set; }
    }

    public class ResetPasswordOTP
    {
        public string ResetCode { get; set; }
        public string EmailAddress { get; set; }
        public string Fullname { get; set; }
    }

    public class TokenReturnDto
    {
        public string AccessToken { get; set; }
        public DateTime? ExpiresIn { get; set; }
    }

}
