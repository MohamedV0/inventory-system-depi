using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Identity;
using InventoryManagementSystem.Models.ViewModels.Notification;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services;

public class UserActivityService : IUserActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContextService _userContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(
        ApplicationDbContext context,
        IUserContextService userContext,
        INotificationService notificationService,
        ILogger<UserActivityService> logger)
    {
        _context = context;
        _userContext = userContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task LogActivityAsync(ActivityType activityType, string action, string entityType, int? entityId, string details)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var username = _userContext.CurrentUser;

            var activity = new UserActivity
            {
                UserId = userId,
                Username = username,
                ActivityType = activityType,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details
            };

            activity.SetCreatedBy(username);

            _context.UserActivities.Add(activity);

            // Create notifications for specific activity types
            switch (activityType)
            {
                case ActivityType.LowStockAlert:
                    await _notificationService.CreateNotificationAsync(
                        new CreateNotificationViewModel
                        {
                            Title = $"Low Stock Alert - {entityType}",
                            Message = details,
                            Type = NotificationType.LowStock,
                            Priority = NotificationPriority.High,
                            ProductId = entityId
                        },
                        cancellationToken: default
                    );
                    break;

                case ActivityType.ProductDeleted:
                case ActivityType.CategoryDeleted:
                case ActivityType.SupplierDeleted:
                    await _notificationService.CreateNotificationAsync(
                        new CreateNotificationViewModel
                        {
                            Title = $"{action} - {entityType}",
                            Message = details,
                            Type = NotificationType.SystemAlert,
                            Priority = NotificationPriority.Medium,
                            ProductId = entityType == "Product" ? entityId : null
                        },
                        cancellationToken: default
                    );
                    break;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User activity logged: {ActivityType} - {Action} on {EntityType} {EntityId} by {Username}",
                activityType, action, entityType, entityId, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error logging user activity: {ActivityType} - {Action} on {EntityType} {EntityId}",
                activityType, action, entityType, entityId);
            throw;
        }
    }

    public async Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(string userId, int? skip = null, int? take = null)
    {
        try
        {
            IQueryable<UserActivity> query = _context.UserActivities
                .Where(ua => ua.UserId == userId && !ua.IsDeleted)
                .OrderByDescending(ua => ua.CreatedAt);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activities for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<UserActivity>> GetActivitiesByEntityAsync(string entityType, int entityId)
    {
        try
        {
            return await _context.UserActivities
                .Where(ua => ua.EntityType == entityType && 
                            ua.EntityId == entityId && 
                            !ua.IsDeleted)
                .OrderByDescending(ua => ua.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for entity {EntityType} {EntityId}", 
                entityType, entityId);
            throw;
        }
    }

    public async Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int count)
    {
        try
        {
            return await _context.UserActivities
                .Where(ua => !ua.IsDeleted)
                .OrderByDescending(ua => ua.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activities");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetActivityStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var activities = await _context.UserActivities
                .Where(ua => !ua.IsDeleted &&
                            ua.CreatedAt >= startDate &&
                            ua.CreatedAt <= endDate)
                .GroupBy(ua => ua.EntityType)
                .Select(g => new
                {
                    EntityType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return activities.ToDictionary(x => x.EntityType, x => x.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity statistics for period {StartDate} to {EndDate}", 
                startDate, endDate);
            throw;
        }
    }

    public async Task<IEnumerable<UserActivity>> GetDashboardActivitiesAsync(int count = 10)
    {
        try
        {
            return await _context.UserActivities
                .Where(ua => !ua.IsDeleted)
                .Include(ua => ua.User)
                .OrderByDescending(ua => ua.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard activities");
            throw;
        }
    }

    public async Task<int> GetTotalActivitiesCountAsync(string userId)
    {
        try
        {
            return await _context.UserActivities
                .CountAsync(ua => ua.UserId == userId && !ua.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total activities count for user {UserId}", userId);
            throw;
        }
    }
} 