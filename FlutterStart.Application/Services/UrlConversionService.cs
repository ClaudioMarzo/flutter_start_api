using FlutterStart.Application.DTO;
using Microsoft.Extensions.Logging;
using FlutterStart.Application.Interfaces;
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
            // Validação adicional se quiser: URL bem-formada
            if (string.IsNullOrWhiteSpace(input.Url))
                throw new ArgumentException("URL inválida", nameof(input.Url));

            _logger.LogInformation("Iniciando conversão de URL: {Url}", input.Url);

            // Chama infraestrutura para executar o yt-dlp
            var result = await _processRunner.RunYtDlpAsync(input.Url, input.Format);

            // Opcional: aqui você pode aplicar lógica de domínio ou pós-processamento adicional.
            // Por exemplo, filtrar mensagens irrelevantes, mapear códigos de erro para mensagens customizadas, etc.
            // Supondo que result seja um objeto com detalhes de output/error/sucesso.

            _logger.LogInformation("Conversão concluída: Success={Success}", result.Success);

            return result;
        }
    }
}
