using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Extensions
{
    /// <summary>
    /// Extension methods for IQueryable to create paged lists
    /// </summary>
    public static class PagedListExtensions
    {
        /// <summary>
        /// Creates a paged list from an IQueryable
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="source">The source IQueryable</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A paged list</returns>
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(
            this IQueryable<T> source, 
            int pageNumber, 
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            var totalCount = await source.CountAsync(cancellationToken);
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new StaticPagedList<T>(items, pageNumber, pageSize, totalCount);
        }
    }
} 