using AuthService.AppCore.Interfaces;
using AuthService.Domain.Dtos;
using AuthService.Domain.Entities;
using AuthService.Persistence.ApplicationContext;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnaxTools.Dto.Http;
using OnaxTools.Enums.Http;

namespace AuthService.AppCore.Repositories
{
    public class UserRoleRepository: IUserRoleRepository
    {
        private readonly ILogger<UserRoleRepository> _logger;
        private readonly AuthDbContext _context;
        private readonly IMapper _mapper;

        public UserRoleRepository(ILogger<UserRoleRepository> logger, AuthDbContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        public async Task<GenResponse<List<RolesResponse>>> GetUserRoles(CancellationToken ct = default)
        {
            List<RolesResponse> objResp = new();
            try
            {
               var theroles = await _context.UserRoles.ToListAsync(ct);
               objResp = _mapper.Map<List<RolesResponse>>(theroles);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return GenResponse<List<RolesResponse>>.Success(objResp);
        }

        public async Task<GenResponse<List<RolesResponse>>> GetUserRolesById(Guid UserId, CancellationToken ct = default)
        {
            List<RolesResponse> objResp = new();
            try
            {
                var theroles = await _context.UserProfileRoles.Where(x=>x.UserId == UserId).ToListAsync(ct);
                if (!theroles.Any()) 
                {
                    return GenResponse<List<RolesResponse>>.Failed("No role maintained for user");
                }
                var roleIds = theroles.Select(x => x.RoleId).ToList();
                var roles = await _context.UserRoles.Where(y => roleIds.Contains(y.Id)).ToListAsync(ct);
                if (!roles.Any())
                {
                    return GenResponse<List<RolesResponse>>.Failed("No roles vailable for user");
                }
                objResp = _mapper.Map<List<RolesResponse>>(roles);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return GenResponse<List<RolesResponse>>.Success(objResp);
        }

        public async Task<GenResponse<int>> AddRolesToUser(RolesToUserCreationDto model, CancellationToken ct = default)
        {
            int countUpdated = 0;
            try
            {
                if (model == null || model.UserRoleIds == null || !model.UserRoleIds.Any())
                {
                    return GenResponse<int>.Failed("Invalid RoleIds passed", StatusCodeEnum.BadRequest);
                }
                List<UserProfileRole> userProfileRoles = new();
                UserProfileRole userProfileRole = new();
                int roles = model.UserRoleIds.Count;
                if (roles > 1)
                {
                    foreach (var m in model.UserRoleIds)
                    {
                        var profiles = new UserProfileRole()
                        {
                            RoleId = m,
                            UserId = model.UserProfileId
                        };
                        userProfileRoles.Add(profiles);
                    };
                    _context.UserProfileRoles.AddRange(userProfileRoles);
                }
                else
                {

                    userProfileRole = new UserProfileRole()
                    {
                        RoleId = model.UserRoleIds.FirstOrDefault(),
                        UserId = model.UserProfileId
                    };
                    _context.UserProfileRoles.Add(userProfileRole);
                }

                countUpdated = await _context.SaveChangesAsync(ct);
                if (countUpdated > 0)
                {
                    _logger.LogInformation("Unable to save user to roles");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.GetType() == typeof(Microsoft.Data.SqlClient.SqlException))
                    {
                        var except = ex.InnerException as Microsoft.Data.SqlClient.SqlException;
                        if (except != null && except.Number == 2601)
                        {
                            return GenResponse<int>.Failed($"Unable to add duplicate role(s). One or more roles already exists for this user.", StatusCodeEnum.NotImplemented);
                        }
                    }
                }
                return GenResponse<int>.Failed("An error occured. Kindly try again.", StatusCodeEnum.ServerError);
            }
            return GenResponse<int>.Success(countUpdated);
        }
    }
}
