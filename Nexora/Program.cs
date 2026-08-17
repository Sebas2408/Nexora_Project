using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexora.Data;
using Nexora.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la cadena
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Servicios
builder.Services.AddDbContext<ApplicationDbContext> ( options =>
    options.UseSqlServer ( connectionString ) );

builder.Services.AddIdentity<ApplicationUser, IdentityRole> ( options =>
{
    // Reglas simples para desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
} )
.AddEntityFrameworkStores<ApplicationDbContext> ()
.AddDefaultTokenProviders ();

builder.Services.AddControllersWithViews ();
builder.Services.AddRazorPages ();

builder.Services.AddSession ( options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromHours ( 2 );
} );

var app = builder.Build();

// Middleware
if ( !app.Environment.IsDevelopment () )
{
    app.UseExceptionHandler ( "/Home/Error" );
    app.UseHsts ();
}

app.UseStaticFiles ();

app.UseRouting ();

app.UseSession ();

app.UseAuthentication ();
app.UseAuthorization ();

// Crear roles al inicio
using ( var scope = app.Services.CreateScope () )
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { "Administrador", "Vendedor", "Cliente" };

    foreach ( var role in roles )
    {
        var exists = await roleManager.RoleExistsAsync(role);
        if ( !exists )
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            // No detener ejecución si falla; en desarrollo se registra en logs
        }
    }
}

// Cuenta administrador de prueba, solo para Desarrollo.
// Nunca se siembra un admin con contraseña fija en producción por seguridad.
if ( app.Environment.IsDevelopment () )
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    const string adminEmail = "admin@nexora.com";
    var adminExistente = await userManager.FindByEmailAsync(adminEmail);

    if ( adminExistente == null )
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Nombre = "Admin",
            Apellido = "Nexora",
            EmailConfirmed = true
        };

        // Contraseña solo válida en local; cámbiala antes de desplegar
        var creado = await userManager.CreateAsync(admin, "Admin123!");
        if ( creado.Succeeded )
        {
            await userManager.AddToRoleAsync ( admin, "Administrador" );
        }
    }
}

// Ruta por defecto
app.MapControllerRoute (
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}" );

// Mapear Razor Pages (necesario para las páginas de Identity en Areas/Identity)
app.MapRazorPages ();

app.Run ();