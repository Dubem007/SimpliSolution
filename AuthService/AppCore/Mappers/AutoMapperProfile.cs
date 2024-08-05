

using AuthService.Domain.Dtos;
using AuthService.Domain.Entities;

namespace AuthService.AppCore.Mappers
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserRole, RolesResponse>().ReverseMap();
        }
    }
}
