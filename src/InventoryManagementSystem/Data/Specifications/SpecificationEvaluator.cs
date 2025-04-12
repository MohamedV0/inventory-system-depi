using System.Linq;
using InventoryManagementSystem.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data.Specifications
{
    /// <summary>
    /// Applies a specification to an IQueryable
    /// </summary>
    public class SpecificationEvaluator<T> where T : BaseEntity
    {
        /// <summary>
        /// Applies a specification to an input queryable and returns the resulting queryable
        /// </summary>
        /// <param name="inputQueryable">The source IQueryable</param>
        /// <param name="specification">The specification to apply</param>
        /// <returns>The resulting IQueryable with the specification applied</returns>
        public static IQueryable<T> GetQuery(IQueryable<T> inputQueryable, ISpecification<T> specification)
        {
            var query = inputQueryable;
            
            // Apply AsNoTracking if specified
            if (specification.AsNoTracking)
                query = query.AsNoTracking();
                
            // Apply criteria
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);
            
            // Apply ordering
            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            
            if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);
            
            // Apply grouping
            if (specification.GroupBy != null)
                query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
            
            // Apply eager loading (includes)
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
            
            // Apply string-based includes
            query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
            
            // Apply paging
            if (specification.IsPagingEnabled)
                query = query.Skip(specification.Skip).Take(specification.Take);
            
            return query;
        }
    }
} 