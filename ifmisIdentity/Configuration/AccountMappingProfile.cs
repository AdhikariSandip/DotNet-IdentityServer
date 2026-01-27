namespace ifmisIdentity.Configuration
{
    using AutoMapper;
    using ifmisIdentity.Dtos;
    using ifmisIdentity.Models;

    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            CreateMap<AccountRegistrationDTO, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.GlobalUserId, opt => opt.MapFrom(src => src.GlobalUserId));

            CreateMap<AccountRegistrationDTO, Organization>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.OrganizationName))
                .ForMember(dest => dest.DatabaseName, opt => opt.MapFrom(src => src.OrganizationDatabaseName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.OrganizationDescription))
                .ForMember(dest => dest.OrgUrl, opt => opt.MapFrom(src => src.OrganizationUrl));
        }
    }

}
