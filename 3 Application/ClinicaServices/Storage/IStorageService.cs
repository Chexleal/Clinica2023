using Microsoft.AspNetCore.Http;

namespace ClinicaServices.Storage;

public interface IStorageService
{
    Task<string> SaveAsync(IFormFile file, string relativePath);
    Task<byte[]> ReadAsync(string relativePath);
    Task DeleteAsync(string relativePath);
    string GetContentType(string fileName);
}
