using InventoryManagementSystem.Models.Common;

namespace InventoryManagementSystem.Extensions
{
    /// <summary>
    /// Extension methods for enums
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Converts a TransactionType enum to its string representation
        /// </summary>
        /// <param name="type">The transaction type enum</param>
        /// <returns>String representation</returns>
        public static string ToDisplayString(this TransactionType type)
        {
            return type switch
            {
                TransactionType.StockIn => "Stock In",
                TransactionType.StockOut => "Stock Out",
                TransactionType.Adjustment => "Adjustment",
                TransactionType.Initial => "Initial Stock",
                TransactionType.Transfer => "Transfer",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Converts a string representation to TransactionType enum
        /// </summary>
        /// <param name="value">String representation of transaction type</param>
        /// <returns>TransactionType enum</returns>
        public static TransactionType ToTransactionType(this string value)
        {
            return value.ToLower() switch
            {
                "stock in" => TransactionType.StockIn,
                "stock out" => TransactionType.StockOut,
                "adjustment" => TransactionType.Adjustment,
                "initial stock" => TransactionType.Initial,
                "transfer" => TransactionType.Transfer,
                _ => TransactionType.Adjustment // Default
            };
        }
    }
} 