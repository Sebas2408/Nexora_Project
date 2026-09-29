using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Nexora.Services;

public sealed class AlmacenamientoImagenTienda
{
    public const string PrefijoUrl = "/uploads/tiendas/";
    private const long TamanoMaximo = 5 * 1024 * 1024;
    private readonly string _directorio;

    public AlmacenamientoImagenTienda(IWebHostEnvironment entorno)
    {
        var raizWeb = entorno.WebRootPath ?? Path.Combine(entorno.ContentRootPath, "wwwroot");
        _directorio = Path.Combine(raizWeb, "uploads", "tiendas");
    }

    public async Task<ResultadoImagenTienda> GuardarAsync(IFormFile? archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return new ResultadoImagenTienda(null, "Selecciona una imagen para la tienda.");
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var tipoEsperado = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };

        if (tipoEsperado == null)
        {
            return new ResultadoImagenTienda(null, "El formato debe ser JPG, PNG o WebP.");
        }

        if (archivo.Length > TamanoMaximo)
        {
            return new ResultadoImagenTienda(null, "La imagen no puede superar los 5 MB.");
        }

        if (!string.IsNullOrWhiteSpace(archivo.ContentType) &&
            archivo.ContentType != "application/octet-stream" &&
            !string.Equals(archivo.ContentType, tipoEsperado, StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoImagenTienda(null, "El tipo de archivo no coincide con su extensión.");
        }

        if (!await TieneFirmaValidaAsync(archivo, extension))
        {
            return new ResultadoImagenTienda(null, "El archivo seleccionado no contiene una imagen válida.");
        }

        Directory.CreateDirectory(_directorio);
        var nombre = $"{Guid.NewGuid():N}{extension}";
        var ruta = Path.Combine(_directorio, nombre);

        await using (var destino = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await archivo.CopyToAsync(destino);
        }

        return new ResultadoImagenTienda($"{PrefijoUrl}{nombre}", null);
    }

    public void Eliminar(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(PrefijoUrl, StringComparison.Ordinal))
        {
            return;
        }

        var nombre = url[PrefijoUrl.Length..];
        if (nombre != Path.GetFileName(nombre) ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(nombre), "N", out _) ||
            Path.GetExtension(nombre) is not (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            return;
        }

        var ruta = Path.Combine(_directorio, nombre);
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }
    }

    private static async Task<bool> TieneFirmaValidaAsync(IFormFile archivo, string extension)
    {
        var cabecera = new byte[12];
        await using var stream = archivo.OpenReadStream();
        var leidos = 0;
        while (leidos < cabecera.Length)
        {
            var cantidad = await stream.ReadAsync(cabecera.AsMemory(leidos));
            if (cantidad == 0) break;
            leidos += cantidad;
        }

        return extension switch
        {
            ".jpg" or ".jpeg" => leidos >= 3 && cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF,
            ".png" => leidos >= 8 && cabecera.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => leidos >= 12 && Encoding.ASCII.GetString(cabecera, 0, 4) == "RIFF" && Encoding.ASCII.GetString(cabecera, 8, 4) == "WEBP",
            _ => false
        };
    }
}

public sealed record ResultadoImagenTienda(string? Url, string? Error)
{
    public bool Correcto => Error == null;
}
