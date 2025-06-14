using FlutterStart.Application.DTO;

namespace FlutterStart.Application.Interfaces;

public interface IUrlConversionService
{
    Task<YtDlpResponseDto> ConvertUrlAsync(InputConvertDto input);
}
