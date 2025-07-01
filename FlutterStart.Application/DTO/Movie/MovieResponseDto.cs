namespace FlutterStart.Application.DTO.Movie;

public class MovieResponseDto
{
    public int Id { get; set; }
    public string? IMDB { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Year { get; set; }
    public string? Language { get; set; }
    public string? DurationMinutes { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public bool IsActive { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }

}