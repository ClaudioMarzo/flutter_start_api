using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using FlutterStart.Application.Services.Interfaces;

namespace FlutterStart.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024;

    public FileStorageService(ILogger<FileStorageService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public bool DeleteFile(string fileUrl)
    {
        _logger.LogInformation($"Tentando deletar o arquivo: {fileUrl}");
        throw new NotImplementedException();
    }

    public async Task<string> SaveImageAsync(IFormFile file, string subfolder)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Arquivo nulo ou vazio recebido para upload.");
            return string.Empty;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning($"Extensão de arquivo não permitida: {extension}");
            throw new InvalidOperationException($"Tipo de arquivo não permitido. Extensões permitidas: {string.Join(", ", _allowedExtensions)}");
        }

        if (file.Length > _maxFileSize)
        {
            _logger.LogWarning($"Arquivo excede o tamanho máximo permitido: {file.Length} bytes");
            throw new InvalidOperationException($"Arquivo muito grande. Tamanho máximo permitido: {_maxFileSize / (1024 * 1024)}MB");
        }

        string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        string uploadsFolder = Path.Combine(_environment.WebRootPath,"images",subfolder);
        if (!Directory.Exists(uploadsFolder))
        {
            _logger.LogInformation($"Criando diretório para uploads: {uploadsFolder}");
            Directory.CreateDirectory(uploadsFolder);
        }

        string filePath = Path.Combine(uploadsFolder, fileName);
        _logger.LogInformation($"Salvando arquivo em: {filePath}");

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            _logger.LogInformation($"Arquivo salvo com sucesso: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao salvar o arquivo: {fileName}");
            throw;
        }

        return $"images/{subfolder}/{fileName}";
    }
}