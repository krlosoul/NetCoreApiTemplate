namespace Core.Dtos.SecretsDto
{
    using Core.Dtos.SerilogDto;

    public class SerilogSecretDto
    {
        public string? MinimumLevel { get; set; }
        public List<WriteToConfigDto>? WriteTo { get; set; }
        public List<string>? Enrich { get; set; }
        public Dictionary<string, string>? Properties { get; set; }
    }
}
