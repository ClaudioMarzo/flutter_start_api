using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FlutterStart.Application.DTO.Movie;

public class MovieUpdateTrailerDto
{
    [Required(ErrorMessage = "O ID do filme é obrigatório.")]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "O arquivo de vídeo é obrigatório.")]
    public IFormFile? TrailerMP4 { get; set; }
}