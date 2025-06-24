namespace FlutterStart.Application.DTO.Book;

public class BookResponseDto
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime PublishedDate { get; set; }
    public string? Genre { get; set; }
    public string? Description { get; set; }
}