using Microsoft.AspNetCore.Http;

namespace FlutterStart.Application.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveImageAsync(IFormFile file, string subfolder);
    bool DeleteFile(string fileUrl);
}