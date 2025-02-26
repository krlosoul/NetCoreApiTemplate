namespace Application.Features.User.Dtos
{
    public class AssignUserRoleDto
    {
        public int UserId { get; set; }
        public IList<int>? RolesId { get; set; }
    }
}