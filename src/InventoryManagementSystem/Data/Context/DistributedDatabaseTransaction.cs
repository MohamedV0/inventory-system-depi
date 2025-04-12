using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Data.Context
{
    /// <summary>
    /// Implementation of IDatabaseTransaction that uses System.Transactions for distributed transactions
    /// </summary>
    public class DistributedDatabaseTransaction : IDatabaseTransaction
    {
        private readonly TransactionScope _transactionScope;
        private readonly ILogger<DistributedDatabaseTransaction> _logger;
        private bool _disposed;
        private bool _completed;

        public DistributedDatabaseTransaction(
            ILogger<DistributedDatabaseTransaction> logger,
            TransactionScopeOption scopeOption = TransactionScopeOption.Required,
            TimeSpan? timeout = null,
            TransactionScopeAsyncFlowOption asyncFlowOption = TransactionScopeAsyncFlowOption.Enabled)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = timeout ?? TransactionManager.DefaultTimeout
            };
            
            _transactionScope = new TransactionScope(scopeOption, options, asyncFlowOption);
            
            _logger.LogInformation("Distributed transaction started with scope option: {ScopeOption}", scopeOption);
        }

        public void Commit()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DistributedDatabaseTransaction));
                
            if (_completed)
                throw new InvalidOperationException("Transaction has already been committed or rolled back.");
                
            try
            {
                _transactionScope.Complete();
                _completed = true;
                _logger.LogInformation("Distributed transaction completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing distributed transaction");
                throw;
            }
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            // TransactionScope doesn't have async methods, so we just call the sync version
            Commit();
            return Task.CompletedTask;
        }

        public void Rollback()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DistributedDatabaseTransaction));
                
            if (_completed)
                throw new InvalidOperationException("Transaction has already been committed or rolled back.");
                
            // For TransactionScope, not calling Complete() effectively rolls back the transaction
            _completed = true;
            _logger.LogInformation("Distributed transaction rolled back");
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            // TransactionScope doesn't have async methods, so we just call the sync version
            Rollback();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (!_completed)
                    {
                        _logger.LogWarning("Distributed transaction disposed without being completed, it will be rolled back");
                    }
                    
                    _transactionScope.Dispose();
                }
                
                _disposed = true;
            }
        }
    }
} 