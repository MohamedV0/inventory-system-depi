using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        private readonly ILogger<AccessDeniedModel> _logger;

        public string ReturnUrl { get; set; }

        public AccessDeniedModel(ILogger<AccessDeniedModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            ReturnUrl = Request.Query["ReturnUrl"].ToString();
            _logger.LogWarning("Access denied attempt for user {UserName} to URL {ReturnUrl}", 
                User.Identity?.Name ?? "Anonymous", 
                !string.IsNullOrEmpty(ReturnUrl) ? ReturnUrl : "Not specified");
        }
    }
} 