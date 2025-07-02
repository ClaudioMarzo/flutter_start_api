using Microsoft.Extensions.Logging;
using FlutterStart.Infrastructure.DTO;
using FlutterStart.Application.Interfaces;
using FlutterStart.Application.DTO.converter;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Application.Services
{
    public class UrlConversionService : IUrlConversionService
    {
        private readonly IProcessRunner _processRunner;
        private readonly ILogger<UrlConversionService> _logger;

        public UrlConversionService(IProcessRunner processRunner, ILogger<UrlConversionService> logger)
        {
            _processRunner = processRunner;
            _logger = logger;
        }

        public async Task<YtDlpResponseDto> ConvertUrlAsync(InputConvertDto input)
        {
            // Validação de negócio
            if (string.IsNullOrWhiteSpace(input.Url))
                throw new ArgumentException("URL inválida", nameof(input.Url));

            if (string.IsNullOrWhiteSpace(input.Format))
                input.Format = "mp4"; // padrão de negócio

            _logger.LogInformation("Iniciando conversão de URL: {Url}", input.Url);

            YtDlpResponseDto result;
            try
            {
                result = await _processRunner.RunYtDlpAsync(input.Url, input.Format);
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

            if (!result.Success)
            {
                // Aqui você pode traduzir erros técnicos em mensagens de negócio, se necessário
                _logger.LogWarning("Conversão falhou: {Error}", result.Error);
            }
            else
            {
                _logger.LogInformation("Conversão concluída: Success={Success}", result.Success);
            }

            return result;
        }
    }
}
