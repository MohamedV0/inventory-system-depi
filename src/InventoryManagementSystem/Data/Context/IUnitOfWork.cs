using System;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Services.Interfaces;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace InventoryManagementSystem.Data.Context
{
    /// <summary>
    /// Unit of Work pattern interface that coordinates operations across multiple repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        // Repository properties
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        ISupplierRepository Suppliers { get; }
        IProductSupplierRepository ProductSuppliers { get; }
        IStockHistoryRepository StockHistories { get; }
        
        // Context access (for advanced scenarios)
        ApplicationDbContext Context { get; }
        IUserContextService UserContext { get; }
        
        /// <summary>
        /// Saves all changes made through the repositories to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of affected rows</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A transaction object that must be committed or rolled back</returns>
        Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Begins a new distributed transaction that can span multiple databases
        /// </summary>
        /// <param name="scopeOption">The transaction scope option</param>
        /// <param name="isolationLevel">The transaction isolation level</param>
        /// <param name="timeout">The transaction timeout</param>
        /// <returns>A transaction object that must be committed or rolled back</returns>
        IDatabaseTransaction BeginDistributedTransaction(
            TransactionScopeOption scopeOption = TransactionScopeOption.Required,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            TimeSpan? timeout = null);
        
        /// <summary>
        /// Executes the provided action within a transaction
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the transaction completed successfully</returns>
        Task<bool> ExecuteInTransactionAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Executes the provided function within a transaction
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="func">The function to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the function if successful, or default if an error occurred</returns>
        Task<TResult?> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> func,
            CancellationToken cancellationToken = default);
    }
} 