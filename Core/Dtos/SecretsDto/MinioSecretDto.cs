namespace Core.Dtos.SecretsDto
{
    public class MinioSecretDto
    {
        public string? Endpoint {get;set;}
        public string? AccessKey {get;set;}
        public string? SecretKey {get;set;}
        public string? BucketName {get;set;}
        public string? FileSizeLimit {get;set;}
    }
}