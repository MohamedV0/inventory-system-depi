using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Data.Context
{
    /// <summary>
    /// Implementation of IDatabaseTransaction that wraps EF Core's IDbContextTransaction
    /// </summary>
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly IDbContextTransaction _transaction;
        private readonly ILogger<DatabaseTransaction> _logger;
        private bool _disposed;
        private bool _completed;

        public DatabaseTransaction(IDbContextTransaction transaction, ILogger<DatabaseTransaction> logger)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Commit()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DatabaseTransaction));
                
            if (_completed)
                throw new InvalidOperationException("Transaction has already been committed or rolled back.");
                
            try
            {
                _transaction.Commit();
                _completed = true;
                _logger.LogInformation("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction");
                throw;
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DatabaseTransaction));
                
            if (_completed)
                throw new InvalidOperationException("Transaction has already been committed or rolled back.");
                
            try
            {
                await _transaction.CommitAsync(cancellationToken);
                _completed = true;
                _logger.LogInformation("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction asynchronously");
                throw;
            }
        }

        public void Rollback()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DatabaseTransaction));
                
            if (_completed)
                throw new InvalidOperationException("Transaction has already been committed or rolled back.");
                
            try
            {
                _transaction.Rollback();
                _completed = true;
                _logger.LogInformation("Transaction rolled back successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction");
                throw;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DatabaseTransaction));
                
            if (_completed)
                throw new InvalidOperationException("Transaction has already been committed or rolled back.");
                
            try
            {
                await _transaction.RollbackAsync(cancellationToken);
                _completed = true;
                _logger.LogInformation("Transaction rolled back successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction asynchronously");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
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
                    // Check if we need to roll back
                    if (!_completed)
                    {
                        try
                        {
                            _transaction.Rollback();
                            _logger.LogWarning("Uncommitted transaction rolled back during disposal");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error rolling back transaction during disposal");
                        }
                    }
                    
                    _transaction.Dispose();
                }
                
                _disposed = true;
            }
        }
        
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                // Check if we need to roll back
                if (!_completed)
                {
                    try
                    {
                        await _transaction.RollbackAsync();
                        _logger.LogWarning("Uncommitted transaction rolled back during async disposal");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error rolling back transaction during async disposal");
                    }
                }
                
                await _transaction.DisposeAsync();
            }
        }
    }
} 