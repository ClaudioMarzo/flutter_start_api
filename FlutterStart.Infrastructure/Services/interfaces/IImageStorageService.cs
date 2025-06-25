using Microsoft.AspNetCore.Http;
using FlutterStart.Infrastructure.DTO;

namespace FlutterStart.Infrastructure.Services.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string subfolder);
}