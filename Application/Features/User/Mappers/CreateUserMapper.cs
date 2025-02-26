namespace Application.Features.User.Mappers
{
    using Core.Entities;
    using AutoMapper;
    using Application.Features.User.Dtos;

    public class CreateUserMapper : Profile
    {
        public CreateUserMapper()
        {
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
        }
    }
}