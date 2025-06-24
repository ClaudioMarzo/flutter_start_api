using Microsoft.AspNetCore.Http;

public interface IFileStorageService
{
    Task<string> SaveImageAsync(IFormFile file, string subfolder);
    bool DeleteFile(string fileUrl);
}