namespace Core.Dtos.SecretsDto
{
    public class  RedisSecretDto
    {
        public string? ConnectionString { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public long MaxLimitBytes { get; set; }
        public long MemoryUsagePercentage { get; set; }
        public bool AllowAdmin { get; set; }
        public int DataBase { get;set; }
    }
}
