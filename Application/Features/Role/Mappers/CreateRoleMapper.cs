namespace Application.Features.Role.Mappers
{
  using Core.Entities;
  using AutoMapper;
  using Application.Features.Role.Dtos;

  public class CreateRoleMapper : Profile
  {
    public CreateRoleMapper()
    {
      CreateMap<CreateRoleDto, Role>()
          .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
    }
  }
}