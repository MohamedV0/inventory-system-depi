using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Controllers
{
    /// <summary>
    /// Controller for managing users, roles, and permissions
    /// </summary>
    [Authorize(Policy = "RequireAdminRole")]
    public class UserManagementController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IUserContextService _userContextService;

        /// <summary>
        /// Constructor
        /// </summary>
        public UserManagementController(
            IUserManagementService userManagementService,
            IUserContextService userContextService)
        {
            _userManagementService = userManagementService;
            _userContextService = userContextService;
        }

        /// <summary>
        /// Display a list of users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchTerm = null, string statusFilter = null, string sortBy = null, bool ascending = true)
        {
            // Set default values for ViewBag properties used in the view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortAscending = ascending;
            ViewBag.PageSize = pageSize;
            
            var users = await _userManagementService.GetUsersAsync();
            var filteredUsers = users.AsEnumerable();
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                filteredUsers = filteredUsers.Where(u => 
                    u.UserName.ToLower().Contains(searchTerm) || 
                    u.Email.ToLower().Contains(searchTerm) || 
                    u.FullName.ToLower().Contains(searchTerm));
            }
            
            // Apply status filter if provided
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (statusFilter.ToLower() == "active")
                {
                    filteredUsers = filteredUsers.Where(u => u.IsActive);
                }
                else if (statusFilter.ToLower() == "inactive")
                {
                    filteredUsers = filteredUsers.Where(u => !u.IsActive);
                }
            }
            
            // Apply sorting if provided
            filteredUsers = sortBy?.ToLower() switch
            {
                "username" => ascending ? filteredUsers.OrderBy(u => u.UserName) : filteredUsers.OrderByDescending(u => u.UserName),
                "email" => ascending ? filteredUsers.OrderBy(u => u.Email) : filteredUsers.OrderByDescending(u => u.Email),
                "fullname" => ascending ? filteredUsers.OrderBy(u => u.FullName) : filteredUsers.OrderByDescending(u => u.FullName),
                "lastlogindate" => ascending ? filteredUsers.OrderBy(u => u.LastLoginDate) : filteredUsers.OrderByDescending(u => u.LastLoginDate),
                _ => filteredUsers.OrderBy(u => u.UserName) // Default sorting
            };
            
            // Apply pagination
            var totalCount = filteredUsers.Count();
            var pagedUsers = filteredUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // Create a StaticPagedList with the filtered and sorted users
            var pagedList = new StaticPagedList<UserViewModel>(pagedUsers, page, pageSize, totalCount);
            
            return View(pagedList);
        }

        /// <summary>
        /// Display details for a user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
                
            return View(user);
        }

        /// <summary>
        /// Display the form for creating a new user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _userManagementService.GetRolesAsync();
            
            var model = new CreateUserViewModel
            {
                AvailableRoles = roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
            };
            
            return View(model);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _userManagementService.GetRolesAsync();
                
                model.AvailableRoles = roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = model.SelectedRoles?.Contains(r.Name) ?? false
                });
                
                return View(model);
            }
            
            var result = await _userManagementService.CreateUserAsync(model);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{model.UserName}' has been successfully created.";
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            var availableRoles = await _userManagementService.GetRolesAsync();
            
            model.AvailableRoles = availableRoles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = model.SelectedRoles?.Contains(r.Name) ?? false
            });
            
            return View(model);
        }

        /// <summary>
        /// Display the form for editing a user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
                
            var roles = await _userManagementService.GetRolesAsync();
            
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                SelectedRoles = user.Roles,
                AvailableRoles = roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = user.Roles.Contains(r.Name)
                })
            };
            
            // Show password fields in the edit form
            ViewBag.ShowPasswordFields = true;
            
            return View(model);
        }

        /// <summary>
        /// Update a user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
                return NotFound();
                
            if (!ModelState.IsValid)
            {
                var roles = await _userManagementService.GetRolesAsync();
                
                model.AvailableRoles = roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = model.SelectedRoles?.Contains(r.Name) ?? false
                });
                
                // Show password fields in the edit form
                ViewBag.ShowPasswordFields = true;
                
                return View(model);
            }
            
            var result = await _userManagementService.UpdateUserAsync(model);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{model.UserName}' has been successfully updated.";
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            var availableRoles = await _userManagementService.GetRolesAsync();
            
            model.AvailableRoles = availableRoles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = model.SelectedRoles?.Contains(r.Name) ?? false
            });
            
            // Show password fields in the edit form
            ViewBag.ShowPasswordFields = true;
            
            return View(model);
        }

        /// <summary>
        /// Display the confirmation page for deleting a user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
                
            return View(user);
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
                
            // Prevent users from deleting their own account
            if (user.Id == _userContextService.GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }
            
            var result = await _userManagementService.DeleteUserAsync(id);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{user.UserName}' has been successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            TempData["ErrorMessage"] = "An error occurred while deleting the user.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Display the permissions management page for a user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
                
            var permissionsByCategory = await _userManagementService.GetUserPermissionsByCategoryAsync(id);
            
            var model = new ManageUserPermissionsViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                PermissionsByCategory = permissionsByCategory
            };
            
            return View(model);
        }

        /// <summary>
        /// Update a user's permissions
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(string id, int[] grantedPermissions)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();
                
            try
            {
                await _userManagementService.UpdateUserPermissionsAsync(id, grantedPermissions ?? Array.Empty<int>());
                
                // If we reach here, the operation was successful
                TempData["SuccessMessage"] = "User permissions have been successfully updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // If we got this far, something failed
                ModelState.AddModelError(string.Empty, ex.Message);
                
                // Reload the user permissions
                var model = await _userManagementService.GetUserPermissionsByCategoryAsync(id);
                if (model == null)
                    return NotFound();
                
                // Get the user info
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound();
                
                var viewModel = new ManageUserPermissionsViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    PermissionsByCategory = model
                };

                TempData["ErrorMessage"] = "An error occurred while updating user permissions.";
                return View(viewModel);
            }
        }

        /// <summary>
        /// Fix permissions for admin users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FixAdminPermissions()
        {
            await _userManagementService.FixAdminPermissionsAsync();
            TempData["SuccessMessage"] = "Admin permissions have been fixed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
} 