using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FlutterStart.Infrastructure.DTO;
using Microsoft.Extensions.Configuration;
using FlutterStart.Infrastructure.Services.Interfaces;

namespace FlutterStart.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string subfolder)
    {
        _logger.LogInformation("Iniciando upload de imagem para o Cloudinary: {FileName}", file.FileName);
        if (file.Length <= 0)
            throw new ArgumentException("Arquivo vazio");

        await using var stream = file.OpenReadStream();
        _logger.LogInformation("Arquivo {FileName} aberto com sucesso, iniciando upload", file.FileName);
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = subfolder
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Falha ao enviar imagem para o Cloudinary: {ErrorMessage}", uploadResult.Error?.Message);
            throw new Exception("Falha ao enviar imagem para o Cloudinary");
        }
        
        _logger.LogInformation("Imagem enviada com sucesso: {Url}", uploadResult.SecureUrl);
        return new ImageUploadResultDto
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }

    public async Task<MovieUploadResultDto> UploadVideoAsync(IFormFile videoFile, string subfolder)
    {
        _logger.LogInformation("Iniciando upload de trailer para o Cloudinary: {FileName}", videoFile.FileName);
        if (videoFile.Length <= 0)
            throw new ArgumentException("Arquivo vazio");

        using var stream = videoFile.OpenReadStream();
        _logger.LogInformation("Arquivo {FileName} aberto com sucesso, iniciando upload", videoFile.FileName);
        
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(videoFile.FileName, stream),
            Folder = subfolder
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Falha ao enviar trailer para o Cloudinary: {ErrorMessage}", uploadResult.Error?.Message);
            throw new Exception("Falha ao trailer imagem para o Cloudinary");
        }

        _logger.LogInformation("Trailer enviada com sucesso: {Url}", uploadResult.SecureUrl);
        return new MovieUploadResultDto
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }
}
