using Microsoft.Extensions.Logging;
using FlutterStart.Infrastructure.DTO;
using FlutterStart.Application.Interfaces;
using FlutterStart.Application.DTO.converter;
using FlutterStart.Infrastructure.Utils.Interfaces;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Application.Services;

public class UrlConversionService : IUrlConversionService
{
    private readonly IUtils _utils;
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<UrlConversionService> _logger;

    public UrlConversionService(IUtils utils, IProcessRunner processRunner, ILogger<UrlConversionService> logger)
    {
        _utils = utils;
        _logger = logger;
        _processRunner = processRunner;
    }

    public async Task<YtDlpResponseDto> ConvertUrlAsync(InputConvertDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Url))
            throw new ArgumentException("URL inválida", nameof(input.Url));

        _logger.LogInformation("Iniciando conversão de URL: {Url}", input.Url);
        try
        {
            // 1 - Obtem caminho do executável yt-dlp e valida se existe
            var exePath = _utils.GetAndValidateYtDlpExecutablePath();
            if (!exePath.Success)
                throw new FileNotFoundException("O executável yt-dlp não foi encontrado no caminho especificado.");

            // Obtem o caminho do executável
            var pathBuilder = exePath.Output;
            
            // 2 - Verifica a permissão da pasta do executável yt-dlp
            var permissionResult = _utils.EnsureExecutablePermission(pathBuilder);
            if (!permissionResult.Success)
                throw new UnauthorizedAccessException("Permissão negada ao executar o comando yt-dlp: " + permissionResult.Message);

            // 3 - Criando diretório de downloads
            var creatFolderResult = _utils.CreatePathDownloadsFolder();
            if (!creatFolderResult.Success)
               throw new InvalidOperationException("Erro ao criar pasta de downloads: " + creatFolderResult.Message);

            // Obtem o caminho para armazenar os downloads
            var downloadFolder = creatFolderResult.Output;

            // 4 - Criando subpasta única para o download
            (bool Success, string UniqueSubfolder, string OutputTemplate, string Error) = _utils.CreateDownloadSubfolder(downloadFolder);
            if (!Success)
                throw new InvalidOperationException("Erro ao criar subpasta de download: " + Error);
            
            // 5 - Obter argumentos de cookies
            string cookiesArg = _utils.GetCookiesArg();
            if (!string.IsNullOrEmpty(cookiesArg))
            {
                _logger.LogInformation("Usando cookies para autenticação no YouTube");
            }
            else
            {
                _logger.LogWarning("Nenhum cookie encontrado - pode haver limitações de rate limiting");
            }
            
            // 6 - Executar yt-dlp com retry automático para rate limiting
            YtDlpResponseDto result = await _processRunner.RunYtDlpWithRetryAsync(
                input.Url, 
                OutputTemplate, 
                UniqueSubfolder, 
                pathBuilder, 
                input.Format, 
                cookiesArg, 
                maxRetries: 3
            );
            
            _logger.LogInformation("Conversão concluída: Success={Success}", result.Success);

            return result;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Executável yt-dlp não encontrado");
            return new YtDlpResponseDto
            {
                Success = false,
                Message = "O executável yt-dlp não foi encontrado.",
                Error = ex.Message
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Permissão negada ao executar yt-dlp");
            return new YtDlpResponseDto
            {
                Success = false,
                Message = "Permissão negada ao executar o comando.",
                Error = ex.Message
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro ao criar pasta de downloads");
            return new YtDlpResponseDto
            {
                Success = false,
                Message = "Erro ao criar pasta de downloads.",
                Error = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro técnico ao executar conversão de URL");
            return new YtDlpResponseDto
            {
                Success = false,
                Message = "Erro interno ao processar a conversão.",
                Error = ex.Message
            };
        }
    }
}

