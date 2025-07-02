using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.InteropServices;
using FlutterStart.Infrastructure.DTO;
using FlutterStart.Infrastructure.Settings;
using FlutterStart.Infrastructure.Utils.Interfaces;

namespace FlutterStart.Infrastructure.Utils;

public class Utils : IUtils
{
    private readonly ILogger<Utils> _logger;
    private readonly YtDlpSettings _settings;
    private readonly IWebHostEnvironment _env;
    public Utils(ILogger<Utils> logger, IOptions<YtDlpSettings> settings, IWebHostEnvironment env)
    {
        _env = env;
        _logger = logger;
        _settings = settings.Value;
    }

    public YtDlpResponseDto GetAndValidateYtDlpExecutablePath()
    {
        bool isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "TRUE";
        _logger.LogInformation("Rodando em container: {IsContainer}", isRunningInContainer);

        string exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp_windows.exe" : "yt-dlp_linux";
        string exePath;

        if (isRunningInContainer)
        {
            exePath = Path.Combine("/app", exeName);
            _logger.LogInformation("Executando em contêiner Docker. Caminho do executável: {ExePath}", exePath);
        }
        else
        {
            var baseDir = AppContext.BaseDirectory;
            var projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName ?? baseDir;
            exePath = Path.Combine(projectRoot, exeName);
            _logger.LogInformation("Executando fora do contêiner. Caminho do executável: {ExePath}", exePath);
        }

        if (!File.Exists(exePath))
        {
            _logger.LogError("[GetAndValidateYtDlpExecutablePath] Executável yt-dlp não encontrado em: {ExePath}", exePath);
            return new YtDlpResponseDto
            {
                Message = "Falha no processamento",
                Success = false,
                Output = string.Empty,
                Error = $"Executável yt-dlp não encontrado em: {exePath}"
            };
        }
        _logger.LogInformation("[GetAndValidateYtDlpExecutablePath] Executável yt-dlp encontrado em: {ExePath}", exePath);
        return new YtDlpResponseDto
        {
            Message = "Executável yt-dlp encontrado",
            Success = true,
            Output = exePath,
            Error = string.Empty
        };
    }

    public YtDlpResponseDto EnsureExecutablePermission(string exePath)
    {
        _logger.LogInformation("[EnsureExecutablePermission] Garantindo permissão de execução para: {ExePath}", exePath);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "/bin/chmod",
                    Arguments = $"+x {exePath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
                if (process?.ExitCode == 0)
                {
                    _logger.LogInformation("[EnsureExecutablePermission] Permissão de execução garantida com sucesso para: {ExePath}", exePath);
                    return new YtDlpResponseDto
                    {
                        Success = true,
                        Message = "Permissão de execução garantida com sucesso.",
                        Output = string.Empty,
                        Error = string.Empty
                    };
                }
                _logger.LogWarning("[EnsureExecutablePermission] Falha ao garantir permissão de execução para yt-dlp. ExitCode: {ExitCode}", process?.ExitCode);
                return new YtDlpResponseDto
                {
                    Success = false,
                    Message = "Falha ao garantir permissão de execução.",
                    Output = string.Empty,
                    Error = $"ExitCode: {process?.ExitCode}"
                };

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EnsureExecutablePermission] Não foi possível garantir permissão de execução para yt-dlp");
                return new YtDlpResponseDto
                {
                    Success = false,
                    Message = "Exceção ao garantir permissão de execução.",
                    Output = string.Empty,
                    Error = ex.Message
                };
            }
        }
        _logger.LogInformation("[EnsureExecutablePermission] Não é necessário garantir permissão de execução em Windows.");
        return new YtDlpResponseDto
        {
            Success = true,
            Message = "No Windows, permissão de execução não é necessária.",
            Output = string.Empty,
            Error = string.Empty
        };
    }


    public YtDlpResponseDto CreatePathDownloadsFolder()
    {
        string downloadFolder = _settings.DownloadFolder;
        if (!Path.IsPathRooted(downloadFolder))
        {
            _logger.LogInformation("Caminho de download relativo, convertendo para absoluto: {DownloadFolder}", downloadFolder);
            downloadFolder = Path.Combine(_env.ContentRootPath, downloadFolder);
        }
        _logger.LogInformation("[CreatePathDownloadsFolder] Criando pasta de downloads em: {DownloadFolder}", downloadFolder);
        try
        {
            Directory.CreateDirectory(downloadFolder);
            _logger.LogInformation("[CreatePathDownloadsFolder] Pasta de downloads criada com sucesso: {DownloadFolder}", downloadFolder);
            return new YtDlpResponseDto
            {
                Success = true,
                Message = "Sucesso ao criar a pasta de downloads",
                Output = downloadFolder,
                Error = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreatePathDownloadsFolder] Erro ao criar a pasta de downloads: {DownloadFolder}", downloadFolder);
            return new YtDlpResponseDto
            {
                Message = "Falha ao criar a pasta de downloads",
                Success = false,
                Output = string.Empty,
                Error = $"Erro ao criar a pasta de downloads: {ex.Message}"
            };
        }
    }

    public (bool Success, string UniqueSubfolder, string OutputTemplate, string Error) CreateDownloadSubfolder(string downloadFolder)
    {
        if (string.IsNullOrWhiteSpace(downloadFolder))
        {
            const string error = "[CreateDownloadSubfolder] O caminho do diretório de download está vazio ou nulo.";
            _logger.LogError(error);
            return (false, string.Empty, string.Empty, error);
        }

        try
        {
            string uniqueSubfolder = Path.Combine(downloadFolder, Guid.NewGuid().ToString());
            Directory.CreateDirectory(uniqueSubfolder);
            string outputTemplate = Path.Combine(uniqueSubfolder, "%(id)s.%(ext)s");
            _logger.LogInformation("[CreateDownloadSubfolder] Template de saída definido como: {OutputTemplate}", outputTemplate);
            return (true, uniqueSubfolder, outputTemplate, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateDownloadSubfolder] Erro ao criar subpasta de download.");
            return (false, string.Empty, string.Empty, ex.Message);
        }
    }

    public string GetCookiesArg()
    {
        // Primeiro, tentar obter cookies de variável de ambiente
        var cookiesFromEnv = Environment.GetEnvironmentVariable("YOUTUBE_COOKIES_BASE64");
        if (!string.IsNullOrEmpty(cookiesFromEnv))
        {
            try
            {
                var cookiesContent = Convert.FromBase64String(cookiesFromEnv);
                var cookiesPath = Path.Combine(Path.GetTempPath(), "cookies.txt");
                File.WriteAllBytes(cookiesPath, cookiesContent);
                _logger.LogInformation("Cookies criados a partir da variável de ambiente: {CookiesPath}", cookiesPath);
                return $"--cookies \"{cookiesPath}\"";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar cookies da variável de ambiente");
            }
        }

        // Tentar localizar cookies em diferentes locais
        var possiblePaths = new[]
        {
            "/app/cookies.txt",
            Path.Combine(AppContext.BaseDirectory, "cookies.txt"),
            Path.Combine(Directory.GetCurrentDirectory(), "cookies.txt"),
            Path.Combine(_env.ContentRootPath, "cookies.txt")
        };

        foreach (var cookiesPath in possiblePaths)
        {
            if (File.Exists(cookiesPath))
            {
                _logger.LogInformation("Arquivo de cookies encontrado em: {CookiesPath}", cookiesPath);
                
                // Verificar se o arquivo não está vazio
                var fileInfo = new FileInfo(cookiesPath);
                if (fileInfo.Length > 0)
                {
                    return $"--cookies \"{cookiesPath}\"";
                }
                else
                {
                    _logger.LogWarning("Arquivo de cookies está vazio: {CookiesPath}", cookiesPath);
                }
            }
        }
        
        _logger.LogWarning("Arquivo de cookies não encontrado em nenhum dos caminhos: {Paths}. Prosseguindo sem cookies.", string.Join(", ", possiblePaths));
        return string.Empty;
    }
}