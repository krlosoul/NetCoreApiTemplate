namespace Core.Dtos.Jwt
{
    public class ClaimDto
	{    
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserRoles { get; set; }
    }
}
