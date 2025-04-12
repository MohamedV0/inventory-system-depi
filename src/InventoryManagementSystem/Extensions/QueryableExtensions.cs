using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace InventoryManagementSystem.Extensions
{
    /// <summary>
    /// Extension methods for IQueryable to enhance query performance
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Creates a paged list from an IQueryable with optimized SQL queries
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="source">The source IQueryable</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A paged list</returns>
        public static async Task<IPagedList<T>> ToPagedListOptimizedAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);
            
            // Execute count query first (with optimizations for EF Core)
            var totalCount = await source
                .TagWith("COUNT_QUERY") // Tag for easier debugging
                .CountAsync(cancellationToken);
                
            // Short-circuit if no items (avoid executing second query)
            if (totalCount == 0)
                return new StaticPagedList<T>(Array.Empty<T>(), pageNumber, pageSize, 0);
                
            // Calculate skip
            var skip = (pageNumber - 1) * pageSize;
            
            // Short-circuit if skip beyond total (avoid executing second query)
            if (skip >= totalCount)
                return new StaticPagedList<T>(Array.Empty<T>(), pageNumber, pageSize, totalCount);
                
            // Execute paging query after applying skip and take
            var items = await source
                .TagWith("PAGING_QUERY") // Tag for easier debugging
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
                
            // Create paged list from the results
            return new StaticPagedList<T>(items, pageNumber, pageSize, totalCount);
        }
        
        /// <summary>
        /// Optimizes an IQueryable by applying AsNoTracking 
        /// and avoiding cartesian explosions for read-only queries
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="source">The source IQueryable</param>
        /// <param name="splitQuery">Whether to use split queries for multiple includes</param>
        /// <returns>Optimized IQueryable</returns>
        public static IQueryable<T> AsOptimizedQuery<T>(
            this IQueryable<T> source,
            bool splitQuery = false) where T : class
        {
            // Always use no tracking for read-only queries
            var query = source.AsNoTracking();
            
            // Use split queries for complex includes to avoid cartesian explosions
            if (splitQuery)
            {
                query = query.AsSplitQuery();
            }
            
            return query;
        }
    }
} 