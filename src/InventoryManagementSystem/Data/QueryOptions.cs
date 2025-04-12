using System;

namespace InventoryManagementSystem.Data
{
    /// <summary>
    /// Standard options for query execution
    /// </summary>
    public class QueryOptions
    {
        /// <summary>
        /// Default options for read-only queries (no tracking, no split query)
        /// </summary>
        public static QueryOptions ReadOnly => new QueryOptions { TrackingEnabled = false, SplitQuery = false };
        
        /// <summary>
        /// Default options for update operations (tracking enabled, no split query)
        /// </summary>
        public static QueryOptions ForUpdate => new QueryOptions { TrackingEnabled = true, SplitQuery = false };
        
        /// <summary>
        /// Default options for complex queries with multiple includes (no tracking, split query)
        /// </summary>
        public static QueryOptions ReadOnlyWithSplitQuery => new QueryOptions { TrackingEnabled = false, SplitQuery = true };
        
        /// <summary>
        /// Whether to enable EF Core change tracking
        /// </summary>
        public bool TrackingEnabled { get; set; } = false;
        
        /// <summary>
        /// Whether to use split queries for queries with multiple includes
        /// </summary>
        /// <remarks>
        /// Split queries can be more efficient for complex queries with multiple includes,
        /// but may result in multiple database roundtrips.
        /// </remarks>
        public bool SplitQuery { get; set; } = false;
        
        /// <summary>
        /// Maximum execution time for the query (null for default)
        /// </summary>
        public TimeSpan? CommandTimeout { get; set; }
    }
} 