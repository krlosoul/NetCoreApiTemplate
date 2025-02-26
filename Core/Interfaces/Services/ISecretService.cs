namespace Core.Interfaces.Services
{
    public interface ISecretService
    {
        /// <summary>
        /// Get secret by name.
        /// </summary>
        /// <typeparam name="T">type of data.</typeparam>
        /// <param name="secretName">The secret name.</param>
        /// <returns>The secret data.</returns>
        Task<T> GetSecretAsync<T>(string? secretName) where T : new();
    }
}