namespace Core.Dtos.SerilogDto
{
    public class WriteToArgsDto
    {
        public List<string>? NodeUris { get; set; }
        public string? IndexFormat { get; set; }
        public bool? AutoRegisterTemplate { get; set; }
        public int? BatchSizeLimit { get; set; }
        public int? ConnectionTimeout { get; set; }
        public int? NumberOfReplicas { get; set; }
        public int? NumberOfShards { get; set; }
        public string? TemplateName { get; set; }
        public string? TypeName { get; set; }
    }
}