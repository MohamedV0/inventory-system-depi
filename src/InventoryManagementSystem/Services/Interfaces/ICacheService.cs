using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service for caching data to improve query performance
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from the cache or executes the factory function if not found
        /// </summary>
        /// <typeparam name="T">The type of value to get</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="factory">The function to execute if not in cache</param>
        /// <param name="expiration">Optional expiration time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The cached or newly created value</returns>
        Task<T> GetOrCreateAsync<T>(
            string key, 
            Func<CancellationToken, Task<T>> factory, 
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        /// <param name="key">The cache key</param>
        void Remove(string key);
        
        /// <summary>
        /// Removes all values with keys starting with the specified prefix
        /// </summary>
        /// <param name="keyPrefix">The key prefix</param>
        void RemoveByPrefix(string keyPrefix);
    }
} 