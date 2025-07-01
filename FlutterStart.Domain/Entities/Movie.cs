using FlutterStart.Entities;

namespace FlutterStart.Domain.Entities;

public class Movie : BaseEntity
{
    public string? IMDB { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Year { get; set; }
    public string? Language { get; set; }
    public string? DurationMinutes { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
}