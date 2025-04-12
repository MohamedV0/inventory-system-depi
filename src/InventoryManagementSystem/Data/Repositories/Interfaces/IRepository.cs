using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Data.Specifications;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using X.PagedList;

namespace InventoryManagementSystem.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : InventoryManagementSystem.Models.Entities.Base.BaseEntity
    {
        Task<Result<T>> GetByIdAsync(int id, CancellationToken cancellationToken = default, bool trackChanges = false);
        Task<Result<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default, QueryOptions? options = null);
        Task<Result<IEnumerable<T>>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, QueryOptions? options = null);
        Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<int>> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a paged collection of entities
        /// </summary>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="filter">Optional filter expression</param>
        /// <param name="includeProperties">Optional related entities to include</param>
        /// <param name="options">Query execution options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A paged collection using X.PagedList</returns>
        Task<Result<IPagedList<T>>> GetPagedAsync(
            int page, 
            int pageSize, 
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            QueryOptions? options = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets a single entity by specification
        /// </summary>
        /// <param name="specification">The specification to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A single entity result</returns>
        Task<Result<T>> FirstOrDefaultAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Finds entities by specification
        /// </summary>
        /// <param name="specification">The specification to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of entities</returns>
        Task<Result<IEnumerable<T>>> FindAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets a paged collection of entities by specification
        /// </summary>
        /// <param name="specification">The specification to apply (includes paging configuration)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A paged collection using X.PagedList</returns>
        Task<Result<IPagedList<T>>> GetPagedAsync(
            ISpecification<T> specification,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Counts entities matching a specification
        /// </summary>
        /// <param name="specification">The specification to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of matching entities</returns>
        Task<Result<int>> CountAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Checks if any entity matches the specification
        /// </summary>
        /// <param name="specification">The specification to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any matching entity exists</returns>
        Task<Result<bool>> AnyAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets a value from the cache or from the database if not present
        /// </summary>
        /// <param name="cacheKey">The key to use for caching</param>
        /// <param name="filter">The filter expression</param>
        /// <param name="includeProperties">Optional related entities to include</param>
        /// <param name="expiration">Optional cache expiration</param>
        /// <param name="options">Query execution options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity collection</returns>
        Task<Result<IEnumerable<T>>> GetOrCacheAsync(
            string cacheKey,
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            TimeSpan? expiration = null,
            QueryOptions? options = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets a single entity from the cache or from the database if not present
        /// </summary>
        /// <param name="cacheKey">The key to use for caching</param>
        /// <param name="id">The entity ID</param>
        /// <param name="includeProperties">Optional related entities to include</param>
        /// <param name="expiration">Optional cache expiration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity</returns>
        Task<Result<T>> GetByIdOrCacheAsync(
            string cacheKey,
            int id,
            string? includeProperties = null,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Invalidates cache entries related to this entity type
        /// </summary>
        void InvalidateCache();
        
        /// <summary>
        /// Invalidates a specific cache entry
        /// </summary>
        /// <param name="cacheKey">The cache key to invalidate</param>
        void InvalidateCacheKey(string cacheKey);
            
        /// <summary>
        /// Gets a queryable source for this repository
        /// </summary>
        /// <param name="options">Query execution options</param>
        /// <returns>IQueryable source</returns>
        IQueryable<T> Query(QueryOptions? options = null);
    }
} 