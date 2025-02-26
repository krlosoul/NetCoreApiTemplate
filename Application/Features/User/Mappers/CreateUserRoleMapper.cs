namespace Application.Features.User.Mappers
{
    using Application.Features.User.Dtos;
    using Core.Entities;
    using AutoMapper;

    public class CreateUserRoleMapper : Profile
    {
      public CreateUserRoleMapper()
      {
        CreateMap<CreateUserRoleDto, UserRole>()
          .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
          .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId));
      }
	}
}