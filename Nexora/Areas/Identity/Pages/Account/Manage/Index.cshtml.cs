using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nexora.Models;

namespace Nexora.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string? Email { get; private set; }
        public string? Nombre { get; private set; }
        public string? Apellido { get; private set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Email = user.Email;
                Nombre = user.Nombre;
                Apellido = user.Apellido;
            }
        }
    }
}
