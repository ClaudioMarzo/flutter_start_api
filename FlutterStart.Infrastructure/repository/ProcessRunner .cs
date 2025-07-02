using System.Text;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using FlutterStart.Infrastructure.DTO;
using FlutterStart.Infrastructure.Settings;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Infrastructure.Repository;

public class ProcessRunner : IProcessRunner
{
    private readonly YtDlpSettings _settings;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProcessRunner> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProcessRunner(IOptions<YtDlpSettings> options, ILogger<ProcessRunner> logger, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _logger = logger;
        _settings = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<YtDlpResponseDto> RunYtDlpAsync(string url, string format = "mp4")
    {
        _logger.LogInformation("Iniciando execução do yt-dlp para URL: {Url} com formato: {Format}", url, format);

        // Detecta ambiente e executável
        var current = AppContext.BaseDirectory;
        _logger.LogInformation("Diretório atual: {CurrentDirectory}", current);

        string exePath;
        bool isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        _logger.LogInformation("Rodando em container: {IsContainer}", isRunningInContainer);

        if (isRunningInContainer)
        {
            var runYtDlp = "yt-dlp_linux";
            exePath = Path.Combine(AppContext.BaseDirectory, runYtDlp);
            if (!File.Exists(exePath))
            {
                exePath = "/app/yt-dlp_linux";
            }
            _logger.LogInformation("Executando em contêiner Docker. Caminho do executável: {ExePath}", exePath);
        }
        else
        {
            try
            {
                var directory = Directory.GetParent(current)?.Parent?.Parent?.Parent?.Parent?.FullName;
                if (directory == null)
                {
                    directory = AppDomain.CurrentDomain.BaseDirectory;
                    _logger.LogWarning("Não foi possível determinar diretório base via GetParent. Usando diretório atual: {Directory}", directory);
                }
                else
                {
                    _logger.LogInformation("Diretório base do projeto: {BaseDirectory}", directory);
                }
                var runYtDlp = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp_windows" : "yt-dlp_linux";
                _logger.LogInformation("Executando em plataforma: {Platform}", RuntimeInformation.OSDescription);
                _logger.LogInformation("Localizando executável yt-dlp: {RunYtDlp} e diretório: {Directory}", runYtDlp, directory);
                exePath = Path.Combine(directory, runYtDlp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao determinar caminho do executável. Usando diretório atual como fallback");
                exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp_windows" : "yt-dlp_linux");
            }
        }

        _logger.LogInformation("Caminho final do executável yt-dlp: {ExePath}", exePath);

        // Tenta tornar executável (Linux)
        try
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(exePath))
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "/bin/chmod",
                    Arguments = $"+x {exePath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível garantir permissão de execução para yt-dlp");
        }

        if (!File.Exists(exePath))
        {
            _logger.LogError("Executável yt-dlp não encontrado em: {ExePath}", exePath);
            return new YtDlpResponseDto
            {
                Message = "Falha no processamento",
                Success = false,
                Output = string.Empty,
                Error = $"Executável yt-dlp não encontrado em: {exePath}"
            };
        }

        string downloadFolder = _settings.DownloadFolder;
        if (!Path.IsPathRooted(downloadFolder))
        {
            _logger.LogInformation("Caminho de download relativo, convertendo para absoluto: {DownloadFolder}", downloadFolder);
            downloadFolder = Path.Combine(_env.ContentRootPath, downloadFolder);
        }
        try
        {
            Directory.CreateDirectory(downloadFolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar a pasta de downloads: {DownloadFolder}", downloadFolder);
            return new YtDlpResponseDto
            {
                Message = "Falha ao criar a pasta de downloads",
                Success = false,
                Output = string.Empty,
                Error = $"Erro ao criar a pasta de downloads: {ex.Message}"
            };
        }

        string uniqueSubfolder = Path.Combine(downloadFolder, Guid.NewGuid().ToString());
        Directory.CreateDirectory(uniqueSubfolder);
        string outputTemplate = Path.Combine(uniqueSubfolder, "%(id)s.%(ext)s");
        _logger.LogInformation("Template de saída definido como: {OutputTemplate}", outputTemplate);

        // Cookies: tenta caminhos comuns
        string cookiesPath = "/app/cookies.txt";
        if (!File.Exists(cookiesPath))
        {
            cookiesPath = Path.Combine(AppContext.BaseDirectory, "cookies.txt");
        }
        string cookiesArg = string.Empty;
        if (File.Exists(cookiesPath))
        {
            cookiesArg = $"--cookies {cookiesPath} ";
            _logger.LogInformation("Arquivo de cookies encontrado em: {CookiesPath}", cookiesPath);
        }
        else
        {
            _logger.LogWarning("Arquivo de cookies não encontrado em: {CookiesPath}. Prosseguindo sem cookies.", cookiesPath);
        }

        string ytDlpArgs;
        if (format == "mp3")
        {
            ytDlpArgs = $"{cookiesArg}--extract-audio --audio-format mp3 -o \"{outputTemplate}\" {url}";
        }
        else
        {
            ytDlpArgs = $"{cookiesArg}-f best -o \"{outputTemplate}\" {url}";
        }

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = ytDlpArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _logger.LogInformation("Executando yt-dlp: {ExePath} {Args}", psi.FileName, psi.Arguments);

        var outputSb = new StringBuilder();
        var errorSb = new StringBuilder();

        using var process = new Process { StartInfo = psi };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                outputSb.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                errorSb.AppendLine(e.Data);
        };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao iniciar o processo yt-dlp");
            return new YtDlpResponseDto
            {
                Message = "Falha ao iniciar o processo",
                Success = false,
                Output = string.Empty,
                Error = ex.Message
            };
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var timeoutMs = _settings.TimeoutSeconds * 1000;
        var exited = await Task.Run(() => process.WaitForExit(timeoutMs));
        if (!exited)
        {
            try
            {
                process.Kill(true);
            }
            catch { }
            _logger.LogWarning("Processo yt-dlp excedeu timeout de {TimeoutSeconds}s", _settings.TimeoutSeconds);
            return new YtDlpResponseDto
            {
                Message = "Timeout no processamento",
                Success = false,
                Output = outputSb.ToString(),
                Error = "Timeout excedido"
            };
        }

        string output = outputSb.ToString();
        string error = errorSb.ToString();

        bool hasError = error.Contains("ERROR:");
        bool hasWarning = error.Contains("WARNING:");

        string? redirectedUrl = Regex.Matches(output, @"Following redirect to (https?://[^\s]+)")
                                        .Cast<Match>()
                                        .LastOrDefault()?.Groups[1].Value;

        string? failureReason = error.Contains("Filename too long")
            ? "Nome do arquivo excede o limite do sistema de arquivos."
            : null;

        string? downloadedFilePath = null;
        try
        {
            _logger.LogInformation("Procurando arquivos baixados em: {UniqueSubfolder}", uniqueSubfolder);
            var files = Directory.GetFiles(uniqueSubfolder);
            if (files.Length > 0)
            {
                downloadedFilePath = files.OrderByDescending(f => File.GetLastWriteTimeUtc(f)).First();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível localizar arquivos gerados");
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        string relativePath = string.Empty;
        if (downloadedFilePath != null)
        {
            var downloadsParts = downloadedFilePath.Split("downloads");
            if (downloadsParts.Length > 1)
            {
                relativePath = downloadsParts[1].TrimStart(Path.DirectorySeparatorChar, '/', '\\');
            }
        }
        _logger.LogInformation("Arquivo baixado localizado: {DownloadedFilePath}, caminho relativo: {RelativePath}", downloadedFilePath, relativePath);

        var response = new YtDlpResponseDto
        {
            Message = hasError ? "Falha no processamento" : "Processamento finalizado",
            FilePath = relativePath, 
            Error = error,
            Success = !hasError,
            HasWarnings = hasWarning,
            RedirectedUrl = redirectedUrl,
            FailureReason = failureReason,
        };

        return response;
    }
}

