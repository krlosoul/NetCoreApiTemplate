namespace Application.Features.User.Dtos
{
    using Application.Features.Role.Dtos;

    public class GetAllUserDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhotoName { get; set; }
        public string? PhotoUrl { get; set; }
        public IEnumerable<RoleDto>? Roles { get; set; }
    }
}