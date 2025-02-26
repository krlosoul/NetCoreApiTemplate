namespace Core.Entities
{
    public partial class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string Password { get; set; } = null!;

        public string PhotoName { get; set; } = null!;

        public DateTime RegistrationDate { get; set; }

        public bool Active { get; set; }
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    }
}