namespace FlutterStart.Application.DTO.Book;

public class BookCreateDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Author { get; set; }
    public int PublicationYear { get; set; }
    public int PageCount { get; set; }
    public string? Publisher { get; set; }
    public string? Edition { get; set; }
    public bool IsRented { get; set; }
    public string? ImageUrl { get; set; }
}