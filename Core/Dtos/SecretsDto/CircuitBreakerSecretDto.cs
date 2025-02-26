namespace Core.Dtos.SecretsDto
{
    public class CircuitBreakerSecretDto
    {        
        public int RetryCount { get; set; }
        public double RetryTime { get; set; }
        public int Timeout { get; set; }
        public int FailCount { get; set; }
        public double FailTime { get; set; }
    }
}