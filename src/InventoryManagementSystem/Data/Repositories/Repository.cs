using System.Linq.Expressions;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.Entities.Base;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;
using InventoryManagementSystem.Extensions;
using InventoryManagementSystem.Data.Specifications;

namespace InventoryManagementSystem.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : InventoryManagementSystem.Models.Entities.Base.BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;
        protected readonly IUserContextService _userContext;
        protected readonly ICacheService? _cacheService;
        private const int MaxPageSize = 100;
        private readonly string _entityTypeName;
        private readonly string _cacheKeyPrefix;

        public Repository(
            ApplicationDbContext context, 
            ILogger<Repository<T>> logger,
            IUserContextService userContext,
            ICacheService? cacheService = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _cacheService = cacheService;
            _dbSet = context.Set<T>();
            _entityTypeName = typeof(T).Name;
            _cacheKeyPrefix = $"entity:{_entityTypeName}:";
        }

        protected IQueryable<T> BaseQuery(
            Expression<Func<T, bool>>? predicate = null,
            string? includeProperties = null,
            QueryOptions? options = null)
        {
            options ??= QueryOptions.ReadOnly;
            
            IQueryable<T> query = _dbSet;
            
            if (!options.TrackingEnabled)
            {
                query = query.AsNoTracking();
            }
            
            if (options.SplitQuery && !string.IsNullOrWhiteSpace(includeProperties) && 
                includeProperties.Contains(","))
            {
                query = query.AsSplitQuery();
            }
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(','))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return query;
        }
        
        protected IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), specification);
        }
        
        public virtual async Task<Result<T>> GetByIdAsync(int id, CancellationToken cancellationToken = default, bool trackChanges = false)
        {
            try
            {
                T? entity;
                
                if (trackChanges)
                {
                    entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
                }
                else
                {
                    entity = await _dbSet
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
                }
                
                return entity == null 
                    ? Result<T>.NotFound(_entityTypeName)
                    : Result<T>.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} by id {Id}", _entityTypeName, id);
                return Result<T>.Failure($"Error retrieving {_entityTypeName}");
            }
        }

        public virtual async Task<Result<IEnumerable<T>>> GetAllAsync(
            CancellationToken cancellationToken = default,
            QueryOptions? options = null)
        {
            try
            {
                var query = BaseQuery(null, null, options);
                var result = await query.ToListAsync(cancellationToken);
                return Result<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving {EntityType} list", _entityTypeName);
                return Result<IEnumerable<T>>.Failure($"Error retrieving {_entityTypeName} list");
            }
        }

        public virtual async Task<Result<IEnumerable<T>>> FindAsync(
            Expression<Func<T, bool>> predicate, 
            CancellationToken cancellationToken = default,
            QueryOptions? options = null)
        {
            try
            {
                var query = BaseQuery(predicate, null, options);
                var result = await query.ToListAsync(cancellationToken);
                return Result<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding {EntityType}", _entityTypeName);
                return Result<IEnumerable<T>>.Failure($"Error finding {_entityTypeName}");
            }
        }

        public virtual async Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                return Result<T>.Failure($"{_entityTypeName} cannot be null");

            try
            {
                entity.SetCreatedBy(_userContext.CurrentUser);
                
                await _dbSet.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                InvalidateCache();
                
                _logger.LogInformation("{EntityType} created with ID {Id}", _entityTypeName, entity.Id);
                return Result<T>.Success(entity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding {EntityType}", _entityTypeName);
                return HandleDbUpdateException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {EntityType}", _entityTypeName);
                return Result<T>.Failure($"Error adding {_entityTypeName}");
            }
        }

        public virtual async Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                return Result<T>.Failure($"{_entityTypeName} cannot be null");

            try
            {
                var existingEntity = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);
                if (existingEntity == null)
                    return Result<T>.NotFound(_entityTypeName);

                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                existingEntity.UpdateAuditFields(_userContext.CurrentUser);
                
                await _context.SaveChangesAsync(cancellationToken);
                
                InvalidateCache();
                InvalidateCacheKey($"{_cacheKeyPrefix}{entity.Id}");

                _logger.LogInformation("{EntityType} updated with ID {Id}", _entityTypeName, entity.Id);
                return Result<T>.Success(existingEntity);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating {EntityType} with ID {Id}", 
                    _entityTypeName, entity.Id);
                return Result<T>.Failure($"The {_entityTypeName} has been modified by another user");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating {EntityType}", _entityTypeName);
                return HandleDbUpdateException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", _entityTypeName, entity.Id);
                return Result<T>.Failure($"Error updating {_entityTypeName}");
            }
        }

        public virtual async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
                if (entity == null)
                    return Result<bool>.NotFound(_entityTypeName);

                entity.Delete(_userContext.CurrentUser);
                await _context.SaveChangesAsync(cancellationToken);
                
                InvalidateCache();
                InvalidateCacheKey($"{_cacheKeyPrefix}{id}");
                
                _logger.LogInformation("{EntityType} with ID {Id} marked as deleted", _entityTypeName, id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", _entityTypeName, id);
                return Result<bool>.Failure($"Error deleting {_entityTypeName}");
            }
        }

        public virtual async Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of {EntityType} with ID {Id}", _entityTypeName, id);
                return Result<bool>.Failure($"Error checking {_entityTypeName} existence");
            }
        }

        public virtual async Task<Result<int>> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                if (predicate != null)
                    query = query.Where(predicate);
                
                var count = await query.CountAsync(cancellationToken);
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting {EntityType}", _entityTypeName);
                return Result<int>.Failure($"Error counting {_entityTypeName}");
            }
        }

        public virtual async Task<Result<IPagedList<T>>> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            QueryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                options ??= QueryOptions.ReadOnly;
                
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));
                
                var query = BaseQuery(filter, includeProperties, options);
                
                var pagedList = await query.ToPagedListAsync(page, pageSize, cancellationToken);
                return Result<IPagedList<T>>.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged {EntityType}", _entityTypeName);
                return Result<IPagedList<T>>.Failure($"Error retrieving paged {_entityTypeName} list");
            }
        }
        
        public virtual async Task<Result<T>> FirstOrDefaultAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = ApplySpecification(specification);
                var entity = await query.FirstOrDefaultAsync(cancellationToken);
                
                return entity == null 
                    ? Result<T>.NotFound(_entityTypeName)
                    : Result<T>.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting first {EntityType} by specification", _entityTypeName);
                return Result<T>.Failure($"Error retrieving {_entityTypeName}");
            }
        }
        
        public virtual async Task<Result<IEnumerable<T>>> FindAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = ApplySpecification(specification);
                var result = await query.ToListAsync(cancellationToken);
                
                return Result<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding {EntityType} by specification", _entityTypeName);
                return Result<IEnumerable<T>>.Failure($"Error finding {_entityTypeName}");
            }
        }
        
        public virtual async Task<Result<IPagedList<T>>> GetPagedAsync(
            ISpecification<T> specification,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));
                
                var query = ApplySpecification(specification);
                
                var pagedList = await query.ToPagedListAsync(page, pageSize, cancellationToken);
                return Result<IPagedList<T>>.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged {EntityType} by specification", _entityTypeName);
                return Result<IPagedList<T>>.Failure($"Error retrieving paged {_entityTypeName} list");
            }
        }
        
        public virtual async Task<Result<int>> CountAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = ApplySpecification(specification);
                var count = await query.CountAsync(cancellationToken);
                
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting {EntityType} by specification", _entityTypeName);
                return Result<int>.Failure($"Error counting {_entityTypeName}");
            }
        }
        
        public virtual async Task<Result<bool>> AnyAsync(
            ISpecification<T> specification, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = ApplySpecification(specification);
                var exists = await query.AnyAsync(cancellationToken);
                
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of {EntityType} by specification", _entityTypeName);
                return Result<bool>.Failure($"Error checking {_entityTypeName} existence");
            }
        }
        
        public virtual async Task<Result<IEnumerable<T>>> GetOrCacheAsync(
            string cacheKey,
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            TimeSpan? expiration = null,
            QueryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (_cacheService == null)
            {
                return await FindAsync(filter ?? (_ => true), cancellationToken, options);
            }
            
            try
            {
                var fullCacheKey = cacheKey.StartsWith(_cacheKeyPrefix) 
                    ? cacheKey 
                    : $"{_cacheKeyPrefix}{cacheKey}";
                    
                var result = await _cacheService.GetOrCreateAsync(
                    fullCacheKey,
                    async (ct) => 
                    {
                        var query = BaseQuery(filter, includeProperties, options);
                        return await query.ToListAsync(ct);
                    },
                    expiration,
                    cancellationToken);
                    
                return Result<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached {EntityType}", _entityTypeName);
                return await FindAsync(filter ?? (_ => true), cancellationToken, options);
            }
        }
        
        public virtual async Task<Result<T>> GetByIdOrCacheAsync(
            string cacheKey,
            int id,
            string? includeProperties = null,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            if (_cacheService == null)
            {
                return await GetByIdAsync(id, cancellationToken);
            }
            
            try
            {
                var fullCacheKey = cacheKey.StartsWith(_cacheKeyPrefix) 
                    ? cacheKey 
                    : $"{_cacheKeyPrefix}{cacheKey}";
                    
                var entity = await _cacheService.GetOrCreateAsync(
                    fullCacheKey,
                    async (ct) => 
                    {
                        var queryOptions = new QueryOptions { TrackingEnabled = false };
                        var query = BaseQuery(e => e.Id == id, includeProperties, queryOptions);
                        return await query.FirstOrDefaultAsync(ct);
                    },
                    expiration,
                    cancellationToken);
                    
                return entity == null 
                    ? Result<T>.NotFound(_entityTypeName)
                    : Result<T>.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached {EntityType} with ID {Id}", _entityTypeName, id);
                return await GetByIdAsync(id, cancellationToken);
            }
        }
        
        public virtual void InvalidateCache()
        {
            if (_cacheService != null)
            {
                _cacheService.RemoveByPrefix(_cacheKeyPrefix);
            }
        }
        
        public virtual void InvalidateCacheKey(string cacheKey)
        {
            if (_cacheService != null)
            {
                var fullCacheKey = cacheKey.StartsWith(_cacheKeyPrefix) 
                    ? cacheKey 
                    : $"{_cacheKeyPrefix}{cacheKey}";
                    
                _cacheService.Remove(fullCacheKey);
            }
        }
        
        public virtual IQueryable<T> Query(QueryOptions? options = null)
        {
            options ??= QueryOptions.ReadOnly;
            var query = _dbSet.AsQueryable();
            
            if (!options.TrackingEnabled)
            {
                query = query.AsNoTracking();
            }
            
            return query;
        }
        
        private Result<T> HandleDbUpdateException(DbUpdateException ex)
        {
            var innerException = ex.InnerException?.Message ?? ex.Message;
            
            if (innerException.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                innerException.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                return Result<T>.Conflict($"A {_entityTypeName} with the same unique identifier already exists");
            }
            
            return Result<T>.Failure($"Database error while working with {_entityTypeName}");
        }
    }
} 