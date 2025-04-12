using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace InventoryManagementSystem.Data.Specifications
{
    /// <summary>
    /// Specification pattern interface for creating reusable, composable query specifications
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// The main criteria/predicate to filter entities
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }
        
        /// <summary>
        /// Navigation properties to be eagerly loaded
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }
        
        /// <summary>
        /// String-based include statements for more complex includes
        /// </summary>
        List<string> IncludeStrings { get; }
        
        /// <summary>
        /// Order ascending expression
        /// </summary>
        Expression<Func<T, object>>? OrderBy { get; }
        
        /// <summary>
        /// Order descending expression
        /// </summary>
        Expression<Func<T, object>>? OrderByDescending { get; }
        
        /// <summary>
        /// Group by expression
        /// </summary>
        Expression<Func<T, object>>? GroupBy { get; }
        
        /// <summary>
        /// Number of entities to take (for pagination)
        /// </summary>
        int Take { get; }
        
        /// <summary>
        /// Number of entities to skip (for pagination)
        /// </summary>
        int Skip { get; }
        
        /// <summary>
        /// Whether paging is enabled
        /// </summary>
        bool IsPagingEnabled { get; }
        
        /// <summary>
        /// Whether to use AsNoTracking for read-only queries
        /// </summary>
        bool AsNoTracking { get; }
    }
} 