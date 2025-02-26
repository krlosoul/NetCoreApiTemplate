namespace Core.Entities
{
    public partial class Role
    {
        public int Id { get; set; }

        public string Description { get; set; } = null!;

        public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    }
}