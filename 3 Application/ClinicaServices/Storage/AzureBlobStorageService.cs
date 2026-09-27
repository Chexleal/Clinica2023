using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace ClinicaServices.Storage;

// Guarda solo RUTA en BD; el binario vive en Azure Blob (contenedor mhsystem-prd).
// Dev local: Azurite (StorageConnectionString=UseDevelopmentStorage=true) o LocalStorageService.
public class AzureBlobStorageService(string connectionString, string containerName = "mhsystem-prd") : IStorageService
{
    private readonly BlobContainerClient _container = InitContainer(connectionString, containerName);

    private static BlobContainerClient InitContainer(string connectionString, string containerName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Falta StorageConnectionString.", nameof(connectionString));
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Falta el nombre del contenedor.", nameof(containerName));

        var service = new BlobServiceClient(connectionString);
        var container = service.GetBlobContainerClient(containerName);
        container.CreateIfNotExists(PublicAccessType.None);
        return container;
    }

    public async Task<string> SaveAsync(IFormFile file, string relativePath)
    {
        var blobName = relativePath.Replace("\\", "/").TrimStart('/');
        var blob = _container.GetBlobClient(blobName);
        using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = GetContentType(file.FileName) });
        return blobName;
    }

    public async Task<byte[]> ReadAsync(string relativePath)
    {
        var blobName = relativePath.Replace("\\", "/").TrimStart('/');
        var blob = _container.GetBlobClient(blobName);
        var result = await blob.DownloadContentAsync();
        return result.Value.Content.ToArray();
    }

    public async Task DeleteAsync(string relativePath)
    {
        var blobName = relativePath.Replace("\\", "/").TrimStart('/');
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync();
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
