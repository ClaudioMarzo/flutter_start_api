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
        // Validação de URL (reproduzindo lógica de controller ou de aplicação se quiser centralizar aqui)
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)  || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
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
        var directory = Directory.GetParent(current)!.Parent!.Parent!.Parent!.Parent!.FullName;
        var runYtDlp = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp_windows" : "yt-dlp_linux";
        var exePath = Path.Combine(directory, runYtDlp);

        // Garantir que a pasta de downloads exista; pode ser relativa ao content root ou absoluta
        string downloadFolder = _settings.DownloadFolder;
        // Se for caminho relativo, torna-se relativo ao base directory:
        if (!Path.IsPathRooted(downloadFolder))
        {
            downloadFolder = Path.Combine(_env.ContentRootPath, downloadFolder);
        }
        Directory.CreateDirectory(downloadFolder);

        // Montar template de saída: ex: downloads/video.%(ext)s ou baseado em ID
        // Para evitar conflito entre execuções simultâneas, podemos usar um subdiretório ou GUID
        string uniqueSubfolder = Path.Combine(downloadFolder, Guid.NewGuid().ToString());
        Directory.CreateDirectory(uniqueSubfolder);
        string outputTemplate = Path.Combine(uniqueSubfolder, "%(id)s.%(ext)s");

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
        if (request != null && downloadedFilePath != null)
        {
            var relativePath = downloadedFilePath
                .Split("downloads")[1]
                .TrimStart(Path.DirectorySeparatorChar, '/', '\\');

            var baseUrl = $"{request.Scheme}://{request.Host}";
            downloadedFilePath = $"{baseUrl}/downloads/{relativePath.Replace('\\', '/')}";
        }

        // Opcional: se quiser incluir caminho ou URL de download no DTO, adicione campo em YtDlpResponseDto e passe aqui.
        var response = new YtDlpResponseDto
        {
            Message = hasError ? "Falha no processamento" : "Processamento finalizado",
            FilePath = downloadedFilePath!,
            Error = error,
            Success = !hasError,
            HasWarnings = hasWarning,
            RedirectedUrl = redirectedUrl,
            FailureReason = failureReason,
            Output = output
        };

        return response;
    }
}

