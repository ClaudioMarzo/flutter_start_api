using FlutterStart.Entities;

namespace FlutterStart.Domain.Entities;

public class Book : BaseEntity
{
    public string? ISBN { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Genre { get; set; }
    public string? Author { get; set; }
    public int PublicationYear { get; set; }
    public int PageCount { get; set; }
    public string? Publisher { get; set; }
    public string? Edition { get; set; }
    public bool IsRented { get; set; }
    public string? ImageUrl { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public string? Dimensions { get; set; }
    public string? Location { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}