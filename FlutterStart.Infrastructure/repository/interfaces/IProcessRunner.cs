using FlutterStart.Infrastructure.DTO;

namespace FlutterStart.Infrastructure.Repository.Interfaces;
public interface IProcessRunner
{
    Task<YtDlpResponseDto> RunYtDlpAsync(string url, string outputTemplate, string uniqueSubfolder, string executPath, string format = "mp4", string? cookiesArg = null);
    Task<YtDlpResponseDto> RunYtDlpWithRetryAsync(string url, string outputTemplate, string uniqueSubfolder, string executPath, string format = "mp4", string? cookiesArg = null, int maxRetries = 3);
}
