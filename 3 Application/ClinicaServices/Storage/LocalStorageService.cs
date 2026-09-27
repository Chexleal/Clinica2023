using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ClinicaServices.Storage;

public class LocalStorageService(IWebHostEnvironment env) : IStorageService
{
    private readonly string _rootPath = InitRoot(env);

    private static string InitRoot(IWebHostEnvironment env)
    {
        var rootPath = Path.Combine(env.WebRootPath, "uploads");
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
        return rootPath;
    }

    public async Task<string> SaveAsync(IFormFile file, string relativePath)
    {
        var fullPath = Path.Combine(_rootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        var dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        // Guardamos ruta relativa estilo estudios/{paciente}/{estudio}/file.dcm
        return relativePath.Replace("\\", "/");
    }

    public async Task<byte[]> ReadAsync(string relativePath)
    {
        var fullPath = Path.Combine(_rootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task DeleteAsync(string relativePath)
    {
        var fullPath = Path.Combine(_rootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".dcm" => "application/dicom",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
