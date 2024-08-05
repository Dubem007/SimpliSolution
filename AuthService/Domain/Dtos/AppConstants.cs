namespace AuthService.Domain.Dtos;

public static class AppConstants
{
    public static readonly string CreationSuccessResponse = "Data created successfully";
    public static readonly string UpdateSuccessResponse = "Data updated successfully";
    public static readonly string LoginSuccessResponse = "Login successful";
    public static readonly string UserNotFound = "User not found";
    public static readonly string PasswordResetSuccessfully = "Password reset successfully";
    public static readonly string PasswordChangedSuccessfully = "Password changed successfully";
    public static readonly string FailedProfileUnlock = "Profile unlock failed";
    public static readonly string ProfileUnlockedSuccessfully = "Profile unclocked successfully";
    public static readonly string ResetCode = "123456";
    public static string System { get; set; } = nameof(System);
}