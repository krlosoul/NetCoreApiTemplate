namespace Core.Interfaces.Services
{
    public interface ICircuitBreakerService
    {
        /// <summary>
        /// Execute action.
        /// </summary>
        /// <typeparam name="T">The action type.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>Result.</returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> action);
    } 
}