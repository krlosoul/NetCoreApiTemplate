namespace Core.Dtos.SecretsDto
{
    public class KeycloakSecretDto
    {
        public string? Audience {get;set;}
        public string? Authority {get;set;}
        public bool RequireHttpsMetadata {get;set;}
    }   
}