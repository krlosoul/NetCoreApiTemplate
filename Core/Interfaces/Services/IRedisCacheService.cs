namespace Core.Interfaces.Services
{
    public interface IRedisCacheService
    {
        /// <summary>
        /// Set or update data by key.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        /// <param name="expiration">The time expiration.</param>
        /// <returns>true if create, false otherwise.</returns>
        Task<bool> SetCacheAsync<T>(string key, T data, TimeSpan? expiration = null);

        /// <summary>
        /// Get data by key.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The data.</returns>
        Task<T?> GetCacheAsync<T>(string key);

        /// <summary>
        /// Delete data by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if delete, false otherwise.</returns>
        Task<bool> DeleteCacheAsync(string key);

        /// <summary>
        /// Validate if exists data by key.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>true if exits, false otherwise.</returns>
        Task<bool> ExistsCacheAsync(string key);

        /// <summary>
        /// Validate memory usage.
        /// </summary>
        /// <returns>Response.</returns>
        Task<string> CheckMemoryUsageAsync();
    }
}