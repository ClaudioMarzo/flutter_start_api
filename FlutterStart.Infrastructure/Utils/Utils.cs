using System.Runtime.InteropServices;

namespace FlutterStart.Infrastructure.U;

public static class Utils
{
    public static string GenerateBuildPath()
    {
        throw new NotImplementedException();
    }

    public static string GenerateUniqueSubdirectory()
    {
        var current = AppContext.BaseDirectory;
        var directory = Directory.GetParent(current)!.Parent!.Parent!.Parent!.Parent!.FullName;
        var runYtDlp = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp.exe" : "yt-dlp";
        return Path.Combine(directory, runYtDlp);

    }

    public static bool IsValidUrl(string url)
    {
        throw new NotImplementedException();
    }
}