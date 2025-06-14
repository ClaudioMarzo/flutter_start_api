namespace FlutterStart.Application.DTO;

public class YtDlpResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? RedirectedUrl { get; set; }
    public string Error { get; set; } = string.Empty;
    public bool HasWarnings { get; set; }
    public string? FailureReason { get; set; }
    public string Output { get; set; } = string.Empty;
}

