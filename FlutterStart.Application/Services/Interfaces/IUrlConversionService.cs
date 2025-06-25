using FlutterStart.Infrastructure.DTO;
using FlutterStart.Application.DTO.converter;

namespace FlutterStart.Application.Interfaces;

public interface IUrlConversionService
{
    Task<YtDlpResponseDto> ConvertUrlAsync(InputConvertDto input);
}
