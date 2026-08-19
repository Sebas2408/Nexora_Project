using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Nexora.Models;

namespace Nexora.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public void OnGet()
        {
            // Intentionally empty - the logout is performed via POST from the layout
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario desconectado.");

            if (string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(Url.Content("~/"));
            }

            return LocalRedirect(returnUrl);
        }
    }
}
