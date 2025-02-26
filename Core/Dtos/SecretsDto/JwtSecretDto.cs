namespace Core.Dtos.SecretsDto
{
    public class JwtSecretDto
    {
        public string? SecretKey { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public double ExpirationTime {get; set; }
    }
}