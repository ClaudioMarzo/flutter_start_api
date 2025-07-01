using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FlutterStart.Application.DTO.Movie;

public class MovieCreateDto
{
    [Required(ErrorMessage = "O IMDB é obrigatório.")]
    public string? IMDB { get; set; }
    
    [Required(ErrorMessage = "O título é obrigatório.")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "O ano é obrigatório.")]
    public int Year { get; set; }

    [Required(ErrorMessage = "O idioma é obrigatório.")]
    public string? Language { get; set; }

    [Required(ErrorMessage = "A duração em minutos é obrigatória.")]
    public string? DurationMinutes { get; set; }

    [Required(ErrorMessage = "O gênero é obrigatório.")]
    public string? Genre { get; set; }

    [Required(ErrorMessage = "O diretor é obrigatório.")]
    public string? Director { get; set; }

    [Required(ErrorMessage = "O elenco é obrigatório.")]
    public string? Cast { get; set; }

    [Required(ErrorMessage = "A duração é obrigatória.")]
    public IFormFile? Poster { get; set; }
}