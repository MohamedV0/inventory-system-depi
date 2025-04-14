using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Identity;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services
{
    /// <summary>
    /// Service for managing users, roles, and permissions
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IUserContextService _userContextService;

        /// <summary>
        /// Constructor
        /// </summary>
        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IUserContextService userContextService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _userContextService = userContextService;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserViewModel>> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await GetUserPermissionsAsync(user.Id);

                result.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    Permissions = permissions.ToList()
                });
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<UserViewModel?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(userId);

            return new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                Roles = roles.ToList(),
                Permissions = permissions.ToList()
            };
        }

        /// <inheritdoc />
        public async Task<IdentityResult> CreateUserAsync(CreateUserViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,  // Use email as username to match normal registration
                Email = model.Email,
                FullName = model.FullName,
                IsActive = true,
                LastLoginDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return result;

            if (model.SelectedRoles.Any())
            {
                // Assign roles
                result = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                if (!result.Succeeded)
                    return result;
                
                // Assign default permissions based on roles
                await AssignDefaultPermissionsForUserAsync(user.Id, model.SelectedRoles);
            }

            return result;
        }

        /// <summary>
        /// Assigns default permissions to a user based on their roles
        /// </summary>
        private async Task AssignDefaultPermissionsForUserAsync(string userId, IEnumerable<string> roles)
        {
            // Get all available permissions
            var allPermissions = await _context.Permissions.ToListAsync();
            if (!allPermissions.Any())
                return;
                
            var permissionIdsToGrant = new List<int>();
            var currentUsername = await _userContextService.GetCurrentUserNameAsync();
            
            // If user is an Admin, grant all permissions
            if (roles.Contains("Admin"))
            {
                permissionIdsToGrant = allPermissions.Select(p => p.Id).ToList();
            }
            // If user is Staff, grant appropriate subset of permissions
            else if (roles.Contains("Staff"))
            {
                // Get all non-admin permissions (exclude user management permissions)
                permissionIdsToGrant = allPermissions
                    .Where(p => 
                        p.Category != "User Management" && 
                        !p.Name.Contains("Delete") &&
                        p.Name != "RequireAdminRole")
                    .Select(p => p.Id)
                    .ToList();
            }
            
            // Grant the permissions
            if (permissionIdsToGrant.Any())
            {
                await UpdateUserPermissionsAsync(userId, permissionIdsToGrant);
            }
        }

        /// <inheritdoc />
        public async Task<IdentityResult> UpdateUserAsync(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result;

            // Update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                // Generate password reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Reset password with the new one
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);
                if (!resetResult.Succeeded)
                    return resetResult;
            }
            
            // Get current roles before updating
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove roles that are no longer selected
            var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToArray();
            if (rolesToRemove.Any())
            {
                result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!result.Succeeded)
                    return result;
            }
            
            // Add newly selected roles
            var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToArray();
            if (rolesToAdd.Any())
            {
                result = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!result.Succeeded)
                    return result;
            }

            // If roles changed, update permissions
            if (rolesToRemove.Any() || rolesToAdd.Any())
            {
                await AssignDefaultPermissionsForUserAsync(model.Id, model.SelectedRoles);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            // Delete user permissions
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();
                
            _context.UserPermissions.RemoveRange(userPermissions);
            await _context.SaveChangesAsync();

            return await _userManager.DeleteAsync(user);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IdentityRole>> GetRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return await _context.Permissions.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserPermissionViewModel>> GetUserPermissionsAsync(string userId)
        {
            // Get all permissions
            var allPermissions = await _context.Permissions.ToListAsync();
            
            // Get user's permissions
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();
                
            // Create view models for all permissions
            var result = new List<UserPermissionViewModel>();
            
            foreach (var permission in allPermissions)
            {
                var userPermission = userPermissions.FirstOrDefault(up => up.PermissionId == permission.Id);
                
                result.Add(new UserPermissionViewModel
                {
                    Id = userPermission?.Id ?? 0,
                    PermissionId = permission.Id,
                    PermissionName = permission.Name,
                    Description = permission.Description,
                    Category = permission.Category,
                    IsGranted = userPermission?.IsGranted ?? false
                });
            }
            
            return result.OrderBy(p => p.Category).ThenBy(p => p.PermissionName);
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, List<UserPermissionViewModel>>> GetUserPermissionsByCategoryAsync(string userId)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            
            return permissions
                .GroupBy(p => p.Category)
                .ToDictionary(
                    g => g.Key, 
                    g => g.ToList()
                );
        }

        /// <inheritdoc />
        public async Task<UserPermission?> GetUserPermissionAsync(string userId, int permissionId)
        {
            return await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);
        }

        /// <inheritdoc />
        public async Task UpdateUserPermissionsAsync(string userId, IEnumerable<int> grantedPermissionIds)
        {
            var grantedIds = grantedPermissionIds.ToList();
            var currentUsername = await _userContextService.GetCurrentUserNameAsync();
            
            // Get all permissions
            var allPermissions = await _context.Permissions.Select(p => p.Id).ToListAsync();
            
            // Get existing user permissions
            var existingUserPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();
                
            // Remove permissions that are not in the granted list
            var permissionsToRemove = existingUserPermissions
                .Where(up => !grantedIds.Contains(up.PermissionId))
                .ToList();
                
            if (permissionsToRemove.Any())
            {
                foreach (var permission in permissionsToRemove)
                {
                    permission.Delete(currentUsername);
                }
            }
            
            // Update existing permissions
            var permissionsToUpdate = existingUserPermissions
                .Where(up => grantedIds.Contains(up.PermissionId))
                .ToList();
                
            if (permissionsToUpdate.Any())
            {
                foreach (var permission in permissionsToUpdate)
                {
                    permission.UpdateAuditFields(currentUsername);
                }
            }
            
            // Add new permissions
            var existingPermissionIds = existingUserPermissions.Select(up => up.PermissionId).ToList();
            var permissionIdsToAdd = grantedIds.Except(existingPermissionIds).ToList();
            
            if (permissionIdsToAdd.Any())
            {
                foreach (var permissionId in permissionIdsToAdd)
                {
                    var newPermission = new UserPermission
                    {
                        UserId = userId,
                        PermissionId = permissionId,
                        IsGranted = true
                    };

                    // Set created by using proper method
                    newPermission.SetCreatedBy(currentUsername);
                    
                    _context.UserPermissions.Add(newPermission);
                }
            }
            
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task CreateDefaultPermissionsAsync()
        {
            // Check if permissions already exist
            if (await _context.Permissions.AnyAsync())
                return;
                
            // Define all the permissions based on the policies in Program.cs
            var permissions = new List<Permission>
            {
                // Basic permissions
                new Permission { Name = "RequireAdminRole", Description = "Admin role access", Category = "Roles" },
                new Permission { Name = "RequireStaffRole", Description = "Staff role access", Category = "Roles" },
                
                // Product permissions
                new Permission { Name = "CanViewProducts", Description = "Can view products", Category = "Products" },
                new Permission { Name = "CanCreateProducts", Description = "Can create products", Category = "Products" },
                new Permission { Name = "CanEditProducts", Description = "Can edit products", Category = "Products" },
                new Permission { Name = "CanDeleteProducts", Description = "Can delete products", Category = "Products" },
                
                // Stock permissions
                new Permission { Name = "CanViewStock", Description = "Can view stock", Category = "Stock" },
                new Permission { Name = "CanAddStock", Description = "Can add stock", Category = "Stock" },
                new Permission { Name = "CanRemoveStock", Description = "Can remove stock", Category = "Stock" },
                new Permission { Name = "CanAdjustStock", Description = "Can adjust stock", Category = "Stock" },
                
                // Category permissions
                new Permission { Name = "CanViewCategories", Description = "Can view categories", Category = "Categories" },
                new Permission { Name = "CanManageCategories", Description = "Can manage categories", Category = "Categories" },
                
                // Supplier permissions
                new Permission { Name = "CanViewSuppliers", Description = "Can view suppliers", Category = "Suppliers" },
                new Permission { Name = "CanManageSuppliers", Description = "Can manage suppliers", Category = "Suppliers" },
                
                // Report permissions
                new Permission { Name = "CanViewReports", Description = "Can view reports", Category = "Reports" },
                new Permission { Name = "CanExportReports", Description = "Can export reports", Category = "Reports" },
                
                // Notification permissions
                new Permission { Name = "CanViewNotifications", Description = "Can view notifications", Category = "Notifications" },
                
                // Dashboard permission
                new Permission { Name = "CanAccessDashboard", Description = "Can access dashboard", Category = "Dashboard" },
                
                // User Management permissions
                new Permission { Name = "CanViewUsers", Description = "Can view users", Category = "User Management" },
                new Permission { Name = "CanCreateUsers", Description = "Can create users", Category = "User Management" },
                new Permission { Name = "CanEditUsers", Description = "Can edit users", Category = "User Management" },
                new Permission { Name = "CanDeleteUsers", Description = "Can delete users", Category = "User Management" },
                new Permission { Name = "CanManagePermissions", Description = "Can manage user permissions", Category = "User Management" }
            };
            
            foreach (var permission in permissions)
            {
                permission.SetCreatedBy("System");
            }
            
            await _context.Permissions.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Fix permissions for all admin users to ensure they have all permissions
        /// </summary>
        public async Task FixAdminPermissionsAsync()
        {
            // Get all users in Admin role
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
                return;
                
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            if (!adminUsers.Any())
                return;
                
            // Get all permissions
            var allPermissions = await _context.Permissions.ToListAsync();
            if (!allPermissions.Any())
                return;
                
            var permissionIds = allPermissions.Select(p => p.Id).ToList();
            var currentUsername = await _userContextService.GetCurrentUserNameAsync();
            
            // For each admin, ensure they have all permissions
            foreach (var adminUser in adminUsers)
            {
                await UpdateUserPermissionsAsync(adminUser.Id, permissionIds);
            }
        }
    }
} 