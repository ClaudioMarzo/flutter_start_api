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

    public async Task<YtDlpResponseDto> RunYtDlpAsync(string url, string outputTemplate, string uniqueSubfolder, string executPath, string format = "mp4", string? cookiesArg = null)
    {
        _logger.LogInformation("Iniciando execução do yt-dlp com URL: {Url}, Formato: {Format}, Template de saída: {OutputTemplate}, caminho de execução {ExecutPath}", url, format, outputTemplate, executPath);

        string ytDlpArgs = BuildYtDlpArguments(url, outputTemplate, format, cookiesArg);
        var psi = BuildProcessStartInfo(executPath, ytDlpArgs);

        var (output, error, processStartError) = await RunProcessAsync(psi);
        if (processStartError != null)
        {
            return new YtDlpResponseDto
            {
                Message = "Falha ao iniciar o processo",
                Success = false,
                Output = string.Empty,
                Error = processStartError
            };
        }

        bool hasError = error.Contains("ERROR:");
        bool hasWarning = error.Contains("WARNING:");
        string? redirectedUrl = ExtractRedirectedUrl(output);
        
        // Detectar diferentes tipos de falhas
        string? failureReason = null;
        if (error.Contains("Filename too long"))
        {
            failureReason = "Nome do arquivo excede o limite do sistema de arquivos.";
        }
        else if (error.Contains("HTTP Error 429"))
        {
            failureReason = "Muitas requisições - YouTube está limitando o acesso. Tente novamente em alguns minutos.";
        }
        else if (error.Contains("Sign in to confirm you're not a bot") || error.Contains("Use --cookies-from-browser"))
        {
            failureReason = "YouTube requer autenticação. Configure cookies do navegador ou use um serviço proxy.";
        }
        else if (error.Contains("Video unavailable"))
        {
            failureReason = "Vídeo não disponível ou foi removido.";
        }
        else if (error.Contains("Private video"))
        {
            failureReason = "Vídeo privado - acesso não autorizado.";
        }
        else if (error.Contains("This video is not available"))
        {
            failureReason = "Vídeo não disponível na sua região.";
        }
        else if (error.Contains("Requested format is not available"))
        {
            failureReason = "Formato solicitado não está disponível para este vídeo.";
        }

        string? downloadedFilePath = TryGetDownloadedFilePath(uniqueSubfolder);
        string relativePath = BuildRelativePath(downloadedFilePath);

        _logger.LogInformation("Arquivo baixado localizado: {DownloadedFilePath}, caminho relativo: {RelativePath}", downloadedFilePath, relativePath);

        return new YtDlpResponseDto
        {
            Message = hasError ? "Falha no processamento" : "Processamento finalizado",
            FilePath = relativePath,
            Error = error,
            Success = !hasError,
            HasWarnings = hasWarning,
            RedirectedUrl = redirectedUrl,
            FailureReason = failureReason,
        };
    }

    private string BuildYtDlpArguments(string url, string outputTemplate, string format, string? cookiesArg)
    {
        var baseArgs = new List<string>();
        
        // Adicionar cookies se disponível
        if (!string.IsNullOrEmpty(cookiesArg))
        {
            baseArgs.Add(cookiesArg.Trim());
        }
        
        // Argumentos essenciais para evitar detecção de bot
        baseArgs.Add("--user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36\"");
        baseArgs.Add("--extractor-args \"youtube:player_client=android\"");
        baseArgs.Add("--add-header \"Accept-Language:en-US,en;q=0.9\"");
        
        // Adicionar argumentos para evitar rate limiting
        baseArgs.Add("--sleep-interval 1");
        baseArgs.Add("--max-sleep-interval 5");
        baseArgs.Add("--sleep-subtitles 1");
        baseArgs.Add("--retries 3");
        baseArgs.Add("--fragment-retries 3");
        baseArgs.Add("--retry-sleep linear=1::2");
        
        // Configurações de formato específicas
        if (format == "mp3")
        {
            baseArgs.Add("--extract-audio");
            baseArgs.Add("--audio-format mp3");
            baseArgs.Add("--audio-quality 0"); // Melhor qualidade de áudio
            baseArgs.Add("--embed-metadata");
        }
        else
        {
            // Priorizar mp4 e evitar webm
            baseArgs.Add("-f \"best[ext=mp4][height<=720]/bestvideo[ext=mp4][height<=720]+bestaudio[ext=m4a]/best[height<=720]\"");
            baseArgs.Add("--merge-output-format mp4");
            baseArgs.Add("--embed-metadata");
        }
        
        baseArgs.Add($"-o \"{outputTemplate}\"");
        baseArgs.Add($"\"{url}\"");
        
        return string.Join(" ", baseArgs);
    }

    private ProcessStartInfo BuildProcessStartInfo(string executPath, string ytDlpArgs)
    {
        _logger.LogInformation("Executando yt-dlp: {ExePath} {Args}", executPath, ytDlpArgs);
        return new ProcessStartInfo
        {
            FileName = executPath,
            Arguments = ytDlpArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    private async Task<(string output, string error, string? processStartError)> RunProcessAsync(ProcessStartInfo psi)
    {
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
            _logger.LogInformation("Iniciando processo yt-dlp com argumentos: {Arguments}", psi.Arguments);
            process.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao iniciar o processo yt-dlp");
            return (string.Empty, string.Empty, ex.Message);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        _logger.LogInformation("Processo yt-dlp iniciado com PID: {ProcessId}", process.Id);
        _logger.LogInformation("Timeout configurado para {TimeoutSeconds}s", _settings.TimeoutSeconds);

        var timeoutMs = _settings.TimeoutSeconds * 1000;
        var exited = await Task.Run(() => process.WaitForExit(timeoutMs));
        if (!exited)
        {
            try
            {
                process.Kill(true);
                _logger.LogWarning("Processo yt-dlp excedeu timeout de {TimeoutSeconds}s", _settings.TimeoutSeconds);
                return (outputSb.ToString(), errorSb.ToString(), "Timeout excedido");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao finalizar processo yt-dlp após timeout");
                return (outputSb.ToString(), errorSb.ToString(), ex.Message);
            }
        }

        // Aguardar um pouco mais para garantir que os arquivos sejam escritos
        await Task.Delay(2000);

        return (outputSb.ToString(), errorSb.ToString(), null);
    }

    private string? ExtractRedirectedUrl(string output)
    {
        return Regex.Matches(output, @"Following redirect to (https?://[^\s]+)")
            .Cast<Match>()
            .LastOrDefault()?.Groups[1].Value;
    }

    private string? TryGetDownloadedFilePath(string uniqueSubfolder)
    {
        try
        {
            _logger.LogInformation("Procurando arquivos baixados em: {UniqueSubfolder}", uniqueSubfolder);
            
            // Tentar várias vezes com delay, pois o arquivo pode demorar para aparecer
            for (int attempt = 0; attempt < 5; attempt++)
            {
                if (Directory.Exists(uniqueSubfolder))
                {
                    var files = Directory.GetFiles(uniqueSubfolder, "*", SearchOption.AllDirectories);
                    
                    // Filtrar arquivos temporários
                    var validFiles = files.Where(f => 
                        !Path.GetFileName(f).StartsWith(".") &&
                        !f.EndsWith(".part") &&
                        !f.EndsWith(".tmp") &&
                        !f.EndsWith(".ytdl")
                    ).ToArray();
                    
                    if (validFiles.Length > 0)
                    {
                        var latestFile = validFiles.OrderByDescending(f => File.GetLastWriteTimeUtc(f)).First();
                        _logger.LogInformation("Arquivo encontrado: {FilePath}", latestFile);
                        return latestFile;
                    }
                    
                    _logger.LogInformation("Tentativa {Attempt}: Nenhum arquivo encontrado ainda, aguardando...", attempt + 1);
                    Thread.Sleep(1000); // Aguardar 1 segundo antes da próxima tentativa
                }
                else
                {
                    _logger.LogWarning("Diretório não existe: {UniqueSubfolder}", uniqueSubfolder);
                    break;
                }
            }
            
            _logger.LogWarning("Nenhum arquivo válido encontrado após 5 tentativas");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível localizar arquivos gerados");
        }
        return null;
    }

    private string BuildRelativePath(string? downloadedFilePath)
    {
        if (downloadedFilePath == null)
            return string.Empty;

        var downloadsParts = downloadedFilePath.Split("downloads");
        if (downloadsParts.Length > 1)
        {
            return downloadsParts[1].TrimStart(Path.DirectorySeparatorChar, '/', '\\');
        }
        return string.Empty;
    }

    public async Task<YtDlpResponseDto> RunYtDlpWithRetryAsync(string url, string outputTemplate, string uniqueSubfolder, string executPath, string format = "mp4", string? cookiesArg = null, int maxRetries = 3)
    {
        var delays = new[] { 1000, 3000, 8000 }; // 1s, 3s, 8s
        
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            _logger.LogInformation("Tentativa {Attempt} de {MaxRetries} para URL: {Url}", attempt + 1, maxRetries, url);
            
            var result = await RunYtDlpAsync(url, outputTemplate, uniqueSubfolder, executPath, format, cookiesArg);
            
            // Se foi bem-sucedido ou não é um erro de rate limiting, retornar o resultado
            if (result.Success || !IsRateLimitError(result.Error))
            {
                return result;
            }
            
            // Se não é a última tentativa, aguardar antes de tentar novamente
            if (attempt < maxRetries - 1)
            {
                var delay = delays[attempt];
                _logger.LogWarning("Rate limit detectado. Aguardando {Delay}ms antes da próxima tentativa", delay);
                await Task.Delay(delay);
            }
        }
        
        // Se todas as tentativas falharam, retornar o último resultado
        return await RunYtDlpAsync(url, outputTemplate, uniqueSubfolder, executPath, format, cookiesArg);
    }

    private bool IsRateLimitError(string error)
    {
        return !string.IsNullOrEmpty(error) && (
            error.Contains("HTTP Error 429") ||
            error.Contains("Too Many Requests") ||
            error.Contains("rate limit") ||
            error.Contains("Sign in to confirm you're not a bot") ||
            error.Contains("Use --cookies-from-browser") ||
            error.Contains("HTTP Error 403") ||
            error.Contains("blocked")
        );
    }

    public async Task<YtDlpResponseDto> RunYtDlpWithFallbackStrategiesAsync(string url, string outputTemplate, string uniqueSubfolder, string executPath, string format = "mp4", string? cookiesArg = null)
    {
        // Estratégia 1: Tentar com cookies (se disponível)
        var result = await RunYtDlpWithRetryAsync(url, outputTemplate, uniqueSubfolder, executPath, format, cookiesArg);
        
        if (result.Success)
        {
            return result;
        }

        // Se falhou devido a problemas de autenticação, tentar estratégias alternativas
        if (IsAuthenticationError(result.Error))
        {
            _logger.LogWarning("Falha de autenticação detectada. Tentando estratégias alternativas...");

            // Estratégia 2: Tentar extrair cookies do navegador automaticamente
            if (string.IsNullOrEmpty(cookiesArg))
            {
                _logger.LogInformation("Tentando extrair cookies do navegador automaticamente...");
                var browserCookiesArg = "--cookies-from-browser chrome";
                result = await RunYtDlpWithRetryAsync(url, outputTemplate, uniqueSubfolder, executPath, format, browserCookiesArg);
                
                if (result.Success)
                {
                    return result;
                }
            }

            // Estratégia 3: Usar player client alternativo (android)
            _logger.LogInformation("Tentando com configurações alternativas de player...");
            var alternativeResult = await RunYtDlpWithAlternativePlayerAsync(url, outputTemplate, uniqueSubfolder, executPath, format);
            
            if (alternativeResult.Success)
            {
                return alternativeResult;
            }

            // Estratégia 4: Tentar com formato mais simples
            if (format != "mp3")
            {
                _logger.LogInformation("Tentando download com formato mais simples...");
                var simpleResult = await RunYtDlpWithSimpleFormatAsync(url, outputTemplate, uniqueSubfolder, executPath, format);
                
                if (simpleResult.Success)
                {
                    return simpleResult;
                }
            }
        }

        // Se todas as estratégias falharam, retornar o resultado original com mensagem melhorada
        result.FailureReason = "Todas as estratégias de download falharam. Verifique se os cookies estão configurados corretamente ou se o vídeo está disponível.";
        return result;
    }

    private async Task<YtDlpResponseDto> RunYtDlpWithAlternativePlayerAsync(string url, string outputTemplate, string uniqueSubfolder, string executPath, string format)
    {
        var baseArgs = new List<string>
        {
            "--extractor-args \"youtube:player_client=android,web\"",
            "--user-agent \"Mozilla/5.0 (Linux; Android 10; SM-G975F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.120 Mobile Safari/537.36\"",
            "--sleep-interval 2",
            "--max-sleep-interval 8",
            "--retries 2"
        };

        if (format == "mp3")
        {
            baseArgs.Add("--extract-audio");
            baseArgs.Add("--audio-format mp3");
            baseArgs.Add("--audio-quality 0");
        }
        else
        {
            baseArgs.Add("-f \"worst[height<=480]/worst\"");
        }

        baseArgs.Add($"-o \"{outputTemplate}\"");
        baseArgs.Add($"\"{url}\"");

        var args = string.Join(" ", baseArgs);
        var psi = BuildProcessStartInfo(executPath, args);
        var (output, error, processStartError) = await RunProcessAsync(psi);

        if (processStartError != null)
        {
            return new YtDlpResponseDto
            {
                Message = "Falha ao iniciar processo alternativo",
                Success = false,
                Error = processStartError
            };
        }

        bool hasError = error.Contains("ERROR:");
        string? downloadedFilePath = TryGetDownloadedFilePath(uniqueSubfolder);
        string relativePath = BuildRelativePath(downloadedFilePath);

        return new YtDlpResponseDto
        {
            Message = hasError ? "Falha no processamento alternativo" : "Download alternativo concluído",
            FilePath = relativePath,
            Error = error,
            Success = !hasError,
            HasWarnings = error.Contains("WARNING:")
        };
    }

    private async Task<YtDlpResponseDto> RunYtDlpWithSimpleFormatAsync(string url, string outputTemplate, string uniqueSubfolder, string executPath, string format)
    {
        var baseArgs = new List<string>
        {
            "--user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36\"",
            "--sleep-interval 3",
            "--retries 1",
            "-f \"worst/best\"", // Formato mais simples
            $"-o \"{outputTemplate}\"",
            $"\"{url}\""
        };

        var args = string.Join(" ", baseArgs);
        var psi = BuildProcessStartInfo(executPath, args);
        var (output, error, processStartError) = await RunProcessAsync(psi);

        if (processStartError != null)
        {
            return new YtDlpResponseDto
            {
                Message = "Falha ao iniciar processo simples",
                Success = false,
                Error = processStartError
            };
        }

        bool hasError = error.Contains("ERROR:");
        string? downloadedFilePath = TryGetDownloadedFilePath(uniqueSubfolder);
        string relativePath = BuildRelativePath(downloadedFilePath);

        return new YtDlpResponseDto
        {
            Message = hasError ? "Falha no processamento simples" : "Download simples concluído",
            FilePath = relativePath,
            Error = error,
            Success = !hasError,
            HasWarnings = error.Contains("WARNING:")
        };
    }

    private bool IsAuthenticationError(string error)
    {
        return !string.IsNullOrEmpty(error) && (
            error.Contains("Sign in to confirm you're not a bot") ||
            error.Contains("Use --cookies-from-browser") ||
            error.Contains("HTTP Error 403") ||
            error.Contains("blocked") ||
            error.Contains("Forbidden")
        );
    }
}

