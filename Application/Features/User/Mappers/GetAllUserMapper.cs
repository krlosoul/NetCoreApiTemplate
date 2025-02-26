namespace Application.Features.User.Mappers
{
    using Application.Features.User.Dtos;
    using Core.Entities;
    using AutoMapper;
    using Application.Features.Role.Dtos;

    public class GetAllUserMapper : Profile
    {
        public GetAllUserMapper()
        {
            CreateMap<UserRole, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Role!.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Role!.Description));

            CreateMap<User, GetAllUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.PhotoName, opt => opt.MapFrom(src => src.PhotoName))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles));
        }
    }
}