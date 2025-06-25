namespace FlutterStart.Application.DTO.Book;

public class LoanResponseDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string? BookISBN { get; set; }
    public string? BookTitle { get; set; }
    public string? UserName { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}