using Microsoft.AspNetCore.Http;

namespace FlutterStart.Application.DTO.Movie;

public class MovieCreateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Year { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public IFormFile Poster { get; set; } = null!;
}