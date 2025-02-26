namespace Infrastructure.Services
{
    using System;
    using System.Linq.Expressions;
    using Hangfire;
    using Application.Interfaces.Services;

    public class HangfireService : IHangfireService
    {
        #region Properties
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        #endregion

        public HangfireService()
        {
            _backgroundJobClient = new BackgroundJobClient();
            _recurringJobManager = new RecurringJobManager();
        }

        public void Enqueue<T>(Expression<Action<T>>  methodCall) => _backgroundJobClient.Enqueue(methodCall);

        public void Schedule<T>(Expression<Action<T>>  methodCall, TimeSpan delay) => _backgroundJobClient.Schedule(methodCall, delay);

        public void RecurringJob<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression) => _recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression);
    }
}