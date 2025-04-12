using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace InventoryManagementSystem.Data.Specifications
{
    /// <summary>
    /// Base implementation of the specification pattern
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected BaseSpecification() 
        {
            Criteria = _ => true; // Default to selecting all
        }
        
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }
        
        public Expression<Func<T, bool>> Criteria { get; protected set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
        public List<string> IncludeStrings { get; } = new List<string>();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public Expression<Func<T, object>>? GroupBy { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; } = false;
        public bool AsNoTracking { get; private set; } = true; // Default to no tracking for better performance
        
        /// <summary>
        /// Adds an include expression for eager loading
        /// </summary>
        /// <param name="includeExpression">Expression to include related entity</param>
        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }
        
        /// <summary>
        /// Adds a string-based include
        /// </summary>
        /// <param name="includeString">Include string (e.g. "Category.Products")</param>
        protected virtual void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }
        
        /// <summary>
        /// Adds paging to the specification
        /// </summary>
        /// <param name="skip">Number of entities to skip</param>
        /// <param name="take">Number of entities to take</param>
        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
        
        /// <summary>
        /// Sets the order by expression (ascending)
        /// </summary>
        /// <param name="orderByExpression">Expression to order by</param>
        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }
        
        /// <summary>
        /// Sets the order by expression (descending)
        /// </summary>
        /// <param name="orderByDescendingExpression">Expression to order by descending</param>
        protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }
        
        /// <summary>
        /// Sets the group by expression
        /// </summary>
        /// <param name="groupByExpression">Expression to group by</param>
        protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }
        
        /// <summary>
        /// Sets whether tracking is enabled for this query
        /// </summary>
        /// <param name="enableTracking">True to enable tracking, false to disable</param>
        protected virtual void EnableTracking(bool enableTracking = true)
        {
            AsNoTracking = !enableTracking;
        }
    }
} 