using InventoryManagementSystem.Models.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// View model for a list of users
    /// </summary>
    public class UserListViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; } = new List<UserViewModel>();
    }

    /// <summary>
    /// View model for a single user
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// The user's ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's username
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the user is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// When the user last logged in
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
        
        /// <summary>
        /// The roles the user belongs to
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
        
        /// <summary>
        /// The permissions assigned to the user
        /// </summary>
        public List<UserPermissionViewModel> Permissions { get; set; } = new List<UserPermissionViewModel>();
    }

    /// <summary>
    /// View model for creating a new user
    /// </summary>
    public class CreateUserViewModel
    {
        /// <summary>
        /// The user's username
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's email
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's full name
        /// </summary>
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Confirmation of the user's password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// The roles to assign to the user
        /// </summary>
        [Display(Name = "Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
        
        /// <summary>
        /// All available roles
        /// </summary>
        public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// View model for editing a user
    /// </summary>
    public class EditUserViewModel
    {
        /// <summary>
        /// The user's ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's username
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's email
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's full name
        /// </summary>
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the user is active
        /// </summary>
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The user's password (optional when editing)
        /// </summary>
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }
        
        /// <summary>
        /// Confirmation of the user's password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
        
        /// <summary>
        /// The roles to assign to the user
        /// </summary>
        [Display(Name = "Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
        
        /// <summary>
        /// All available roles
        /// </summary>
        public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// View model for a user permission
    /// </summary>
    public class UserPermissionViewModel
    {
        /// <summary>
        /// The ID of the user permission
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The ID of the permission
        /// </summary>
        public int PermissionId { get; set; }
        
        /// <summary>
        /// The name of the permission
        /// </summary>
        public string PermissionName { get; set; } = string.Empty;
        
        /// <summary>
        /// A description of the permission
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The category the permission belongs to
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the permission is granted
        /// </summary>
        public bool IsGranted { get; set; }
    }
    
    /// <summary>
    /// View model for permission management
    /// </summary>
    public class ManageUserPermissionsViewModel
    {
        /// <summary>
        /// The user's ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's username
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// The user's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// All permissions grouped by category
        /// </summary>
        public Dictionary<string, List<UserPermissionViewModel>> PermissionsByCategory { get; set; } = new Dictionary<string, List<UserPermissionViewModel>>();
    }
} 