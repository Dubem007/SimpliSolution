using AuthService.Domain.Dtos;
using AuthService.Domain.Entities;
using OnaxTools.Dto.Http;

namespace AuthService.AppCore.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<GenResponse<List<RolesResponse>>> GetUserRoles(CancellationToken ct = default);
        Task<GenResponse<List<RolesResponse>>> GetUserRolesById(Guid UserId, CancellationToken ct = default);
        Task<GenResponse<int>> AddRolesToUser(RolesToUserCreationDto model, CancellationToken ct = default);
    }
}
