using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Identity;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IUserActivityService
{
    Task LogActivityAsync(ActivityType activityType, string action, string entityType, int? entityId, string details);

    Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(string userId, int? skip = null, int? take = null);
    Task<IEnumerable<UserActivity>> GetActivitiesByEntityAsync(string entityType, int entityId);
    Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int count);
    Task<Dictionary<string, int>> GetActivityStatisticsAsync(DateTime startDate, DateTime endDate);
    
    Task<IEnumerable<UserActivity>> GetDashboardActivitiesAsync(int count = 10);
    Task<int> GetTotalActivitiesCountAsync(string userId);
} 