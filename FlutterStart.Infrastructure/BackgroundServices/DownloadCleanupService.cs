using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DownloadCleanupService : BackgroundService
{
    private readonly ILogger<DownloadCleanupService> _logger;
    private readonly string _downloadsPath;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _expireAfter = TimeSpan.FromMinutes(5);

    public DownloadCleanupService(ILogger<DownloadCleanupService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _downloadsPath = Path.Combine(env.ContentRootPath, "downloads");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CleanOldFilesAndFolders();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cleanup] Erro ao limpar arquivos antigos.");
            }

            await Task.Delay(_scanInterval, stoppingToken);
        }
    }

    private void CleanOldFilesAndFolders()
    {
        if (!Directory.Exists(_downloadsPath))
            return;

        var now = DateTime.UtcNow;

        // Limpa arquivos diretamente na pasta raiz downloads/
        var files = Directory.GetFiles(_downloadsPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (now - fileInfo.LastWriteTimeUtc > _expireAfter)
            {
                try
                {
                    fileInfo.Delete();
                    _logger.LogInformation($"[Cleanup] Arquivo removido: {fileInfo.FullName}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[Cleanup] Falha ao remover arquivo: {fileInfo.FullName}");
                }
            }
        }

        // Limpa subpastas
        var subdirs = Directory.GetDirectories(_downloadsPath);
        foreach (var dir in subdirs)
        {
            var dirInfo = new DirectoryInfo(dir);
            if (now - dirInfo.LastWriteTimeUtc > _expireAfter)
            {
                try
                {
                    dirInfo.Delete(true);
                    _logger.LogInformation($"[Cleanup] Pasta removida: {dirInfo.FullName}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[Cleanup] Falha ao remover pasta: {dirInfo.FullName}");
                }
            }
        }
    }
}
