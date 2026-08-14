using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Nexora.Models;

namespace Nexora.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string ReturnUrl { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [StringLength(50)]
            public string Nombre { get; set; }

            [Required]
            [StringLength(50)]
            public string Apellido { get; set; }

            public string? Genero { get; set; }

            [DataType(DataType.Date)]
            public DateTime? FechaNacimiento { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, Nombre = Input.Nombre, Apellido = Input.Apellido };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario creado con contraseña.");

                    // Asignar rol por defecto Cliente si existe
                    if (await _userManager.IsInRoleAsync(user, "Cliente") == false)
                    {
                        // Añadir rol 'Cliente' si existe en la base de datos
                        try
                        {
                            await _userManager.AddToRoleAsync(user, "Cliente");
                        }
                        catch
                        {
                            // Ignorar si no existe el rol; rol se crea al inicio de la app
                        }
                    }

                    // Añadir claims para datos opcionales (sin requerir cambios en el modelo)
                    if (!string.IsNullOrEmpty(Input.Genero))
                    {
                        await _userManager.AddClaimAsync(user, new Claim("Genero", Input.Genero));
                    }
                    if (Input.FechaNacimiento.HasValue)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("FechaNacimiento", Input.FechaNacimiento.Value.ToString("yyyy-MM-dd")));
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
