using Microsoft.AspNetCore.Http;
using FlutterStart.Infrastructure.DTO;

namespace FlutterStart.Infrastructure.Services.Interfaces;

public interface ICloudinaryService
{
    Task<MovieUploadResultDto> UploadVideoAsync(IFormFile videoFile, string subfolder);
    Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string subfolder);
}