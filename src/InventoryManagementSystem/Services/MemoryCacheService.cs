using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InventoryManagementSystem.Services
{
    /// <summary>
    /// Implementation of ICacheService using IMemoryCache
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, bool> _keys = new();
        private readonly MemoryCacheEntryOptions _defaultOptions;
        
        /// <summary>
        /// Default cache expiration (10 minutes)
        /// </summary>
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);
        
        public MemoryCacheService(
            IMemoryCache cache, 
            ILogger<MemoryCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _defaultOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(DefaultExpiration)
                .SetPriority(CacheItemPriority.Normal)
                .RegisterPostEvictionCallback(EvictionCallback);
        }
        
        /// <inheritdoc />
        public async Task<T> GetOrCreateAsync<T>(
            string key, 
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
                
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
                
            // See if the item exists in the cache
            if (_cache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue!;
            }
            
            // If not, create it with the factory
            _logger.LogDebug("Cache miss for key: {Key}", key);
            var value = await factory(cancellationToken);
            
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
                Priority = _defaultOptions.Priority
            };
            options.RegisterPostEvictionCallback(EvictionCallback);
            
            // Store in cache
            _cache.Set(key, value, options);
            _keys.TryAdd(key, true);
            
            return value;
        }
        
        /// <inheritdoc />
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
                
            _logger.LogDebug("Removing cache entry with key: {Key}", key);
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
        
        /// <inheritdoc />
        public void RemoveByPrefix(string keyPrefix)
        {
            if (string.IsNullOrEmpty(keyPrefix))
                throw new ArgumentNullException(nameof(keyPrefix));
                
            var keysToRemove = _keys.Keys.Where(k => k.StartsWith(keyPrefix)).ToList();
            
            foreach (var key in keysToRemove)
            {
                _logger.LogDebug("Removing cache entry with key: {Key}", key);
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }
            
            _logger.LogInformation("Removed {Count} cache entries with prefix: {Prefix}", 
                keysToRemove.Count, keyPrefix);
        }
        
        private void EvictionCallback(object key, object? value, EvictionReason reason, object? state)
        {
            _logger.LogDebug("Cache entry with key {Key} was evicted due to {Reason}", key, reason);
            _keys.TryRemove(key.ToString() ?? string.Empty, out _);
        }
    }
} 