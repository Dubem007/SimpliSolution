using AuthService.Domain.Dtos;
using OnaxTools.Dto.Http;

namespace AuthService.AppCore.Interfaces
{
    public interface IAuthRepository
    {
        Task<GenResponse<UserCreationResponseDto>> CreateUser(UserCreationRequestDto input);
        Task<GenResponse<UserLoginResponse>> Login(UserLoginDto model);
        Task<GenResponse<object>> UnlockAccount(UnclockAccountDto model);
        Task<GenResponse<object>> ChangePassword(ChangePasswordDto model);
    }
}
