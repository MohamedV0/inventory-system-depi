using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InventoryManagementSystem.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string CurrentUser
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    // Try to get the username claim
                    var username = user.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(username))
                        return username;

                    // Fallback to email if username is not available
                    var email = user.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                        return email;
                }

                // Return a default value if no authenticated user is found
                return "System";
            }
        }

        public string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return userId;
            }

            // Return a default system user ID if no authenticated user is found
            return "system";
        }
    }
} 