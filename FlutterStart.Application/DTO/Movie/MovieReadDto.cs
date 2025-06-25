namespace FlutterStart.Application.DTO.Movie;

public class MovieReadDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Year { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
}