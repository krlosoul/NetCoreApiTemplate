namespace Application.Interfaces.Services
{
    using System;
    using System.Linq.Expressions;

    public interface IHangfireService
    {
        /// <summary>
        /// Enqueues a background job for immediate execution.
        /// </summary>
        /// <typeparam name="T">The type of the service or class containing the method to execute.</typeparam>
        /// <param name="methodCall">The method to be executed in the background.</param>
        void Enqueue<T>(Expression<Action<T>>  methodCall);

        /// <summary>
        /// Schedules a background job to be executed after a specified delay.
        /// </summary>
        /// <typeparam name="T">The type of the service or class containing the method to execute.</typeparam>
        /// <param name="methodCall">The method to be executed in the background.</param>
        /// <param name="delay">The time span after which the job should execute.</param>
        void Schedule<T>(Expression<Action<T>>  methodCall, TimeSpan delay);

        /// <summary>
        /// Configures a recurring background job to be executed based on a specified CRON expression.
        /// </summary>
        /// <typeparam name="T">The type of the service or class containing the method to execute.</typeparam>
        /// <param name="jobId">A unique identifier for the recurring job.</param>
        /// <param name="methodCall">The method to be executed on a recurring basis.</param>
        /// <param name="cronExpression">A CRON expression defining the schedule for the recurring job.</param>
        void RecurringJob<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
    }
}