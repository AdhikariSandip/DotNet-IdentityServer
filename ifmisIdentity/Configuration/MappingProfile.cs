using AutoMapper;
using ifmisIdentity.Dtos;
using ifmisIdentity.Models;


namespace ifmisIdentity.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<User, UpdateUserDTO>();
            CreateMap<Role, RoleDTO>();
            CreateMap<Organization, OrganizationDTO>();
            CreateMap<UserRole, UserRoleDTO>();
        }

    }

}
