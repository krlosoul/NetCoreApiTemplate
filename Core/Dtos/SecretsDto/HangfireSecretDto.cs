namespace Core.Dtos.SecretsDto
{
    public class HangfireSecretDto
    {
        public string? CompatibilityLevel { get; set; }
        public string? ConnectionString { get; set; }
        public double CommandBatchMaxTimeoutInMinutes { get; set; }
        public double SlidingInvisibilityTimeoutInMinutes { get; set; }
        public double QueuePollIntervalInMilliseconds { get; set; }
        public bool UseRecommendedIsolationLevel { get; set; }
        public bool DisableGlobalLocks { get; set; }
        public string? DashboardUrl { get; set; }
    }
}