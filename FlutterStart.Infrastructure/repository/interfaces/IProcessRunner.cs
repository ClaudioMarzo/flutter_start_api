using FlutterStart.Infrastructure.DTO;

namespace FlutterStart.Infrastructure.Repository.Interfaces;
public interface IProcessRunner
{
    Task<YtDlpResponseDto> RunYtDlpAsync(string url, string format = "mp4");
}
