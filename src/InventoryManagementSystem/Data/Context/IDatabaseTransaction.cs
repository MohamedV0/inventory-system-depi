using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Context
{
    /// <summary>
    /// Abstracts transaction operations to support both local and distributed transactions
    /// </summary>
    public interface IDatabaseTransaction : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Commits the transaction
        /// </summary>
        void Commit();
        
        /// <summary>
        /// Commits the transaction asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        void Rollback();
        
        /// <summary>
        /// Rolls back the transaction asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
} 