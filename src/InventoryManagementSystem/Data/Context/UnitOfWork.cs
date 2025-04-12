using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Services.Interfaces;

namespace InventoryManagementSystem.Data.Context
{
    /// <summary>
    /// Implementation of the Unit of Work pattern that coordinates operations across multiple repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContextService _userContext;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILogger<DatabaseTransaction> _dbTransactionLogger;
        private readonly ILogger<DistributedDatabaseTransaction> _distTransactionLogger;
        private bool _disposed;

        public ICategoryRepository Categories { get; }
        public IProductRepository Products { get; }
        public ISupplierRepository Suppliers { get; }
        public IProductSupplierRepository ProductSuppliers { get; }
        public IStockHistoryRepository StockHistories { get; }
        
        public ApplicationDbContext Context => _context;
        public IUserContextService UserContext => _userContext;

        public UnitOfWork(
            ApplicationDbContext context,
            IUserContextService userContext,
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository,
            IProductSupplierRepository productSupplierRepository,
            IStockHistoryRepository stockHistoryRepository,
            ILogger<UnitOfWork> logger,
            ILogger<DatabaseTransaction> dbTransactionLogger,
            ILogger<DistributedDatabaseTransaction> distTransactionLogger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbTransactionLogger = dbTransactionLogger ?? throw new ArgumentNullException(nameof(dbTransactionLogger));
            _distTransactionLogger = distTransactionLogger ?? throw new ArgumentNullException(nameof(distTransactionLogger));
            
            Categories = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            Products = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            Suppliers = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
            ProductSuppliers = productSupplierRepository ?? throw new ArgumentNullException(nameof(productSupplierRepository));
            StockHistories = stockHistoryRepository ?? throw new ArgumentNullException(nameof(stockHistoryRepository));
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict detected while saving changes");
                throw new ApplicationException("A concurrency conflict occurred while saving changes. The operation was cancelled.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving changes");
                throw new ApplicationException("An error occurred while saving changes to the database.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving changes");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogInformation("Database transaction started");
                return new DatabaseTransaction(transaction, _dbTransactionLogger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting database transaction");
                throw new ApplicationException("Failed to start database transaction", ex);
            }
        }

        /// <inheritdoc />
        public IDatabaseTransaction BeginDistributedTransaction(
            TransactionScopeOption scopeOption = TransactionScopeOption.Required,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            TimeSpan? timeout = null)
        {
            try
            {
                // Create transaction options
                var options = new TransactionOptions
                {
                    IsolationLevel = isolationLevel,
                    Timeout = timeout ?? TransactionManager.DefaultTimeout
                };
                
                // Enable the transaction to flow across async calls
                var transaction = new DistributedDatabaseTransaction(
                    _distTransactionLogger,
                    scopeOption,
                    timeout,
                    TransactionScopeAsyncFlowOption.Enabled);
                
                _logger.LogInformation("Distributed transaction started with isolation level {IsolationLevel}", isolationLevel);
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting distributed transaction");
                throw new ApplicationException("Failed to start distributed transaction", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExecuteInTransactionAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
                
            using var transaction = await BeginTransactionAsync(cancellationToken);
            try
            {
                await action();
                await SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action in transaction");
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<TResult?> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> func,
            CancellationToken cancellationToken = default)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
                
            using var transaction = await BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await func();
                await SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function in transaction");
                await transaction.RollbackAsync(cancellationToken);
                return default;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                
                _disposed = true;
            }
        }
        
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
            }
        }
    }
} 