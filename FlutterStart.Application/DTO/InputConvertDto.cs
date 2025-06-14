using System.ComponentModel.DataAnnotations;

namespace FlutterStart.Application.DTO;

public class InputConvertDto
{
    [Required]
    public string Url { get; set; } = string.Empty;
    [Required]
    [RegularExpression(@"^(mp3|mp4)$", ErrorMessage = "Formato inválido. Use 'mp3' ou 'mp4'.")]
    public string Format { get; set; } = "mp4";

}