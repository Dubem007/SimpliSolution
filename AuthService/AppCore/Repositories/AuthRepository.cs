using AuthService.AppCore.Interfaces;
using AuthService.Domain.Dtos;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Persistence.ApplicationContext;
using AutoMapper;
using Common.Domain;
using Common.Services.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnaxTools.Dto.Http;
using OnaxTools.Enums.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AuthService.AppCore.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;

        public AuthRepository(AuthDbContext context, UserManager<User> userManager, IUserRoleRepository userRoleRepo, IMapper mapper, IConfiguration configuration, ICacheService cacheService)
        {
            _context = context;
            _userManager = userManager;
            _userRoleRepo = userRoleRepo;
            _mapper = mapper;
            _configuration = configuration;
            _cacheService = cacheService;
        }

        public async Task<GenResponse<UserCreationResponseDto>> CreateUser(UserCreationRequestDto input)
        {
            var newUser = new User
            {
                UserName = input.Username.ToLower().Trim(),
                Email = input.EmailAddress.ToLower().Trim(),
                EmailAddress = input.EmailAddress.ToLower().Trim(),
                EmailConfirmed = true,
                Firstname = input.Firstname,
                Middlename = input.Middlename,
                Surname = input.Surname,
                PhoneNumber = input.PhoneNumber,
                CreatedBy = input.Username.ToLower().Trim(),
                CreatedOn = DateTime.Now,
                IsActive = true,
                Status = UserStatus.Approved.ToString(),
            };
            newUser.PasswordHash = _userManager.PasswordHasher.HashPassword(newUser, input.Password);
            var result = await _userManager.CreateAsync(newUser, input.Password);
            if (!result.Succeeded)
            {
                return GenResponse<UserCreationResponseDto>.Failed(
                    $"New User creation failed. [{result.Errors.FirstOrDefault().Description}]");
            }

            var allroles = await _userRoleRepo.GetUserRoles();
            if (!allroles.IsSuccess)
            {
                return GenResponse<UserCreationResponseDto>.Failed(
                    $"No roles avaialable. [{result.Errors.FirstOrDefault().Description}]");
            }

            var rolesReq = new RolesToUserCreationDto()
            {
                UserProfileId = newUser.Id,
                UserRoleIds = input.RoleIds
            };
            var roles = await _userRoleRepo.AddRolesToUser(rolesReq);

            if (roles.Result == 0)
            {
                return GenResponse<UserCreationResponseDto>.Failed(
                    $"New User role creation failed. [{result.Errors.FirstOrDefault().Description}]");
            }

            var allUserRoles = allroles.Result.Where(x => input.RoleIds.Contains(x.Id)).ToList();
            var response = new UserCreationResponseDto
            {
                UserId = newUser.Id,
                Firstname = newUser.Firstname,
                Middlename = newUser.Middlename,
                Surname = newUser.Surname,
                EmailAddress = newUser.EmailAddress,
                PhoneNumber = newUser.PhoneNumber,
                IsActive = newUser.IsActive,
                Status = UserStatus.Approved.ToString(),
                Roles = allUserRoles,
            };

            return new GenResponse<UserCreationResponseDto>
            {
                IsSuccess = true,
                Message = AppConstants.CreationSuccessResponse,
                Result = response,
                StatCode = (int)StatusCodeEnum.Created
            };
        }

        public async Task<GenResponse<UserLoginResponse>> Login(UserLoginDto model)
        {
            var retries = 0;
            var tokenResponse = new TokenReturnDto();
            var user = await _userManager.FindByNameAsync(model.Username.ToLower().Trim());

            if (user == null)
                return GenResponse<UserLoginResponse>.Failed("This user account does not exist.", StatusCodeEnum.NotFound);

            if (user.Status.Equals(UserStatus.Locked.ToString(), StringComparison.OrdinalIgnoreCase))
                return GenResponse<UserLoginResponse>.Failed("This user account has been locked", StatusCodeEnum.Forbidden);

            if (!user.Status.Equals(UserStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (user.Retries > 0 && user.Status != UserStatus.Pending.ToString())
                {
                    return GenResponse<UserLoginResponse>.Failed(
                                   "This user account details has been modified and awaiting approval. Please Contact Admin",
                                   StatusCodeEnum.Forbidden);
                }
            }

            if (!user.IsActive)
                return GenResponse<UserLoginResponse>.Failed("Your Account has been disabled please Contact Admin",
                    StatusCodeEnum.Forbidden);

            var isUserValid = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (isUserValid == PasswordVerificationResult.Failed && user.Retries <= 2)
            {
                user.Retries += 1;
                await _userManager.UpdateAsync(user);
                return GenResponse<UserLoginResponse>.Failed("Invalid Username or Password provided");
            }

            if (user.Retries == 3)
            {
                user.Status = UserStatus.Locked.ToString();
                await _userManager.UpdateAsync(user);
                return GenResponse<UserLoginResponse>.Failed("User profile is now locked");
            }

            var roles = await _userRoleRepo.GetUserRolesById(user.Id);
            var rolenames = roles.Result.Select(x => x.RoleName).ToList();
            tokenResponse = Authenticate(user, rolenames);

            var userViewModel = new UserLoginResponse
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiresIn = tokenResponse.ExpiresIn,
                RefreshToken = GenerateRefreshToken(user.Id),
                Roles = roles.Result,
                UserStatus = user.Status,
            };

            var token_key = CacheKey.GetTokenKey(user.Id);
            await _cacheService.RemoveData(token_key);
            await _cacheService.SetData<string>(token_key, userViewModel.AccessToken, 60 * GetTokenExpireIn());

            return new GenResponse<UserLoginResponse>
            {
                IsSuccess = true,
                Message = AppConstants.LoginSuccessResponse,
                Result = userViewModel,
                StatCode = (int)StatusCodeEnum.OK
            };
        }

        public async Task<GenResponse<object>> ChangePassword(ChangePasswordDto model)
        {
            var userMember = await _userManager.FindByNameAsync(model.Username);
            if (userMember is null)
                return GenResponse<object>.Failed("This user account does not exist.", StatusCodeEnum.NotFound);
            var isPassword = _userManager.PasswordHasher.VerifyHashedPassword(userMember, userMember.PasswordHash, model.OldPassword);
            if (isPassword != PasswordVerificationResult.Success)
                return GenResponse<object>.Failed("Invalid old password provided as password is not recognised");
            if (model.OldPassword.Trim() == model.NewPassword.Trim())
                return GenResponse<object>.Failed("The old and new passwords are same, kindly retry.", StatusCodeEnum.NotFound);

            var isPasswordCorrect = _userManager.PasswordHasher.VerifyHashedPassword(userMember, userMember.PasswordHash, model.NewPassword);
            if (isPasswordCorrect == PasswordVerificationResult.Success)
                return GenResponse<object>.Failed("Invalid new password provided as password already used");

            userMember.PasswordHash = _userManager.PasswordHasher.HashPassword(userMember, model.NewPassword);
            var code = await _userManager.GeneratePasswordResetTokenAsync(userMember);
            var result = await _userManager.ResetPasswordAsync(userMember, code, model.NewPassword);
            if (!result.Succeeded)
                return GenResponse<object>.Failed("Failed Password Change");
           
            return new GenResponse<object>
            {
                IsSuccess = true,
                Message = AppConstants.PasswordChangedSuccessfully,
                Result = null,
                StatCode = (int)StatusCodeEnum.OK
            };
        }

        public async Task<GenResponse<object>> UnlockAccount(UnclockAccountDto model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user is null)
                    return GenResponse<object>.Failed("This user account does not exist.", StatusCodeEnum.NotFound);
                if (user.Status != UserStatus.Locked.ToString())
                {
                    return GenResponse<object>.Failed("This user account is not locked", StatusCodeEnum.Forbidden);
                }

                if (model.ResetCode == AppConstants.ResetCode)
                {
                    user.Retries = 0;
                    user.Status = UserStatus.Approved.ToString();
                    await _userManager.UpdateAsync(user);

                    return new GenResponse<object>
                    {
                        IsSuccess = true,
                        Message = AppConstants.ProfileUnlockedSuccessfully,
                        Result = null,
                        StatCode = (int)StatusCodeEnum.OK
                    };
                }
                else
                {
                    return GenResponse<object>.Failed("Wrong reset code provided, kindly retry", StatusCodeEnum.Forbidden);
                }

                return new GenResponse<object>
                {
                    IsSuccess = false,
                    Message = AppConstants.FailedProfileUnlock,
                    Result = null,
                    StatCode = (int)StatusCodeEnum.BadRequest
                };
            }
            catch (Exception ex)
            {
                return new GenResponse<object>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Result = null
                };
            }
        }

        private TokenReturnDto Authenticate(User user, IList<string> roles)
        {
            var roleClaims = new List<Claim>();
            var claims = new List<Claim>();
   
            claims = new List<Claim>
            {
                new(ClaimTypeHelper.Email, user.Email),
                new(ClaimTypeHelper.UserId, user.Id.ToString()),
                new(ClaimTypeHelper.UserName, user.UserName),
                new(ClaimTypeHelper.Role, JsonSerializer.Serialize(roles), JsonClaimValueTypes.JsonArray),
            };
           

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var jwtUserSecret = jwtSettings.GetSection("Secret").Value;
            var tokenExpireIn = GetTokenExpireIn();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtUserSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(tokenExpireIn),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return new TokenReturnDto
            {
                ExpiresIn = tokenDescriptor.Expires,
                AccessToken = jwt
            };
        }

        private string GenerateRefreshToken(Guid userId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var jwtUserSecret = jwtSettings.GetSection("Secret").Value;
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(jwtUserSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new(ClaimTypeHelper.UserId, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }
        private int GetTokenExpireIn()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenLifeSpan = jwtSettings.GetSection("TokenLifeSpan").Value;
            var tokenExpireIn = string.IsNullOrEmpty(tokenLifeSpan) ? 10 : int.Parse(tokenLifeSpan);
            return tokenExpireIn;
        }
    }
}
