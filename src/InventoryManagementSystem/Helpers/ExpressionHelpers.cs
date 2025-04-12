using System;
using System.Linq.Expressions;

namespace InventoryManagementSystem.Helpers
{
    /// <summary>
    /// Helper class for working with expressions
    /// </summary>
    public static class ExpressionHelpers
    {
        /// <summary>
        /// Combines two expressions using the AND operator
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <param name="expr1">The first expression</param>
        /// <param name="expr2">The second expression</param>
        /// <returns>A combined expression</returns>
        public static Expression<Func<T, bool>> AndAlso<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            // Get the parameter from the first expression
            var parameter = expr1.Parameters[0];
            
            // Create a visitor to replace parameters in the second expression
            var visitor = new ParameterReplacer(expr2.Parameters[0], parameter);
            
            // Replace parameters in the second expression body
            var body2WithParam1 = visitor.Visit(expr2.Body);
            
            // Combine the bodies with the AndAlso (&&) operator
            var combinedBody = Expression.AndAlso(expr1.Body, body2WithParam1);
            
            // Create a new lambda expression with the combined body
            return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
        }
        
        /// <summary>
        /// Parameter replacer visitor for expression trees
        /// </summary>
        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;
            
            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }
            
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
    }
} 