using System.Text;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using FlutterStart.Application.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
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

        // Validação de URL: Verifica se é uma URL válida e se o esquema é http/https
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            _logger.LogWarning("URL inválida ou esquema não suportado: {Url}", url);
            return new YtDlpResponseDto
            {
                Message = "URL inválida",
                Success = false,
                Output = string.Empty,
                Error = "Esquema inválido. Deve iniciar com http/https."
            };
        }

        // Localizar o executável yt-dlp: assumindo que está na raiz do projeto ou em um local conhecido
        var current = AppContext.BaseDirectory;
        _logger.LogInformation("Diretório atual: {CurrentDirectory}", current);
        
        string exePath;
        
        // Verificar se está rodando em um container Docker
        bool isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        _logger.LogInformation("Rodando em container: {IsContainer}", isRunningInContainer);
        
        if (isRunningInContainer)
        {
            // Em ambiente Docker, o executável estará no diretório da aplicação
            var runYtDlp = "yt-dlp_linux"; // Containers Docker rodam Linux
            exePath = Path.Combine(AppContext.BaseDirectory, runYtDlp);
            _logger.LogInformation("Executando em contêiner Docker. Caminho do executável: {ExePath}", exePath);
        }
        else
        {
            // Em ambiente de desenvolvimento
            try
            {
                var directory = Directory.GetParent(current)?.Parent?.Parent?.Parent?.Parent?.FullName;
                if (directory == null)
                {
                    // Fallback para o diretório atual se não conseguir navegar para cima
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
                // Se algo der errado com a navegação de diretórios, tente usar o diretório atual
                _logger.LogError(ex, "Erro ao determinar caminho do executável. Usando diretório atual como fallback");
                exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp_windows" : "yt-dlp_linux");
            }
        }
        
        _logger.LogInformation("Caminho final do executável yt-dlp: {ExePath}", exePath);
        
        // Verificar se o arquivo existe
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
        
        // Garantir que a pasta de downloads exista; pode ser relativa ao content root ou absoluta
        string downloadFolder = _settings.DownloadFolder;
        if (!Path.IsPathRooted(downloadFolder))
        {
            _logger.LogInformation("Caminho de download relativo, convertendo para absoluto: {DownloadFolder}", downloadFolder);
            downloadFolder = Path.Combine(_env.ContentRootPath, downloadFolder);
        }
        Directory.CreateDirectory(downloadFolder);

        // Montar template de saída: ex: downloads/video.%(ext)s ou baseado em ID
        // Para evitar conflito entre execuções simultâneas, podemos usar um subdiretório ou GUID
        string uniqueSubfolder = Path.Combine(downloadFolder, Guid.NewGuid().ToString());
        Directory.CreateDirectory(uniqueSubfolder);
        string outputTemplate = Path.Combine(uniqueSubfolder, "%(id)s.%(ext)s");
        _logger.LogInformation("Template de saída definido como: {OutputTemplate}", outputTemplate);
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = format == "mp3"
                    ? $"--extract-audio --audio-format mp3 -o \"{outputTemplate}\" {url}"
                    : $"-f best -o \"{outputTemplate}\" {url}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _logger.LogInformation("Executando yt-dlp: {ExePath} {Args}", psi.FileName, string.Join(' ', psi.ArgumentList));

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

        // Aguarda com timeout
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

        // Exemplo de extração de redirect (mesma lógica original)
        string? redirectedUrl = Regex.Matches(output, @"Following redirect to (https?://[^\s]+)")
                                        .Cast<Match>()
                                        .LastOrDefault()?.Groups[1].Value;

        string? failureReason = error.Contains("Filename too long")
            ? "Nome do arquivo excede o limite do sistema de arquivos."
            : null;

        // Localizar arquivo gerado: assume que executamos em uma pasta isolada uniqueSubfolder
        string? downloadedFilePath = null;
        try
        {
            _logger.LogInformation("Procurando arquivos baixados em: {UniqueSubfolder}", uniqueSubfolder);
            var files = Directory.GetFiles(uniqueSubfolder);
            if (files.Length > 0)
            {
                // Pega o arquivo mais recente
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

