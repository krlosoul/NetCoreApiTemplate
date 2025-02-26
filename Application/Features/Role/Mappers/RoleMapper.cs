namespace Application.Features.Role.Mappers
{
  using Core.Entities;
  using AutoMapper;
  using Application.Features.Role.Dtos;

  public class RoleMapper : Profile
  {
    public RoleMapper()
    {
      CreateMap<Role, RoleDto>()
         .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
         .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
    }
  }
}