namespace Infrastructure.Services
{
    using Core.Interfaces.Services;
    using Core.Dtos.SecretsDto;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.CircuitBreaker;
    using Polly.Fallback;
    using Polly.Retry;
    using Polly.Timeout;
    using System;

    public class CircuitBreakerService(CircuitBreakerSecretDto circuitBreakerSecretDto, ILogger<CircuitBreakerService> logger) : ICircuitBreakerService
    {
        #region Properties 
        private readonly CircuitBreakerSecretDto _circuitBreakerSecretDto = circuitBreakerSecretDto;
        private readonly ILogger<CircuitBreakerService> _logger = logger;

        #endregion

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            var fallbackPolicy = GetFallbackPolicy<T>();
            var circuitBreakerPolicy = GetCircuitBreakerPolicy();
            var retryPolicy = GetRetryPolicy();
            var timeoutPolicy = GetTimeoutPolicy();

            return await fallbackPolicy
                .WrapAsync(circuitBreakerPolicy)
                .WrapAsync(retryPolicy)
                .WrapAsync(timeoutPolicy)
                .ExecuteAsync(action);
        }

        private AsyncRetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: _circuitBreakerSecretDto.RetryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(_circuitBreakerSecretDto.RetryTime, attempt)),
                    onRetry: (exception, timespan, context) =>
                    {
                        _logger.LogWarning($"Retrying due to: {exception.Message}", exception);
                    });
        }

        private AsyncTimeoutPolicy GetTimeoutPolicy()
        {
            return Policy
                .TimeoutAsync(
                    TimeSpan.FromMilliseconds(_circuitBreakerSecretDto.Timeout), 
                    TimeoutStrategy.Pessimistic, 
                    (context, timespan, task) =>
                    {
                        _logger.LogWarning("Operation exceeded the timeout limit.");
                        return Task.CompletedTask;
                    });
        }

        private AsyncCircuitBreakerPolicy GetCircuitBreakerPolicy()
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _circuitBreakerSecretDto.FailCount,
                    durationOfBreak:  TimeSpan.FromSeconds(_circuitBreakerSecretDto.FailTime),
                    onBreak: (exception, timespan) =>
                    {
                        _logger.LogWarning($"Circuit opened due to: {exception.Message}",exception);
                    },
                    onReset: () =>
                    {
                        _logger.LogWarning("Circuit closed. Calls can go through again.");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogWarning("Circuit half-open. A single call will be tested.");
                    });
        }
        
        private AsyncFallbackPolicy<T> GetFallbackPolicy<T>()
        {
            return Policy<T>
                .Handle<Exception>()
                .FallbackAsync((cancellationToken) =>
                {
                    _logger.LogWarning("Fallback invoked as all attempts failed.");
                    return Task.FromResult(default(T)!);
                });
        }
    }
}