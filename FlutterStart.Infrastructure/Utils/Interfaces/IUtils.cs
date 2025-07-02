using FlutterStart.Infrastructure.DTO;

namespace FlutterStart.Infrastructure.Utils.Interfaces;

public interface IUtils
{
    YtDlpResponseDto GetAndValidateYtDlpExecutablePath();
    YtDlpResponseDto EnsureExecutablePermission(string exePath);
    YtDlpResponseDto CreatePathDownloadsFolder();
    (bool Success, string UniqueSubfolder, string OutputTemplate, string Error) CreateDownloadSubfolder(string downloadFolder);
    string GetCookiesArg();
}