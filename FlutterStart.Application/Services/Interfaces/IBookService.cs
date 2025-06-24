using FlutterStart.Application.DTO;
using FlutterStart.Application.DTO.Book;

namespace FlutterStart.Application.Services.Interfaces;

public interface IBookService
{
    Task<IEnumerable<BookResponseDto>> GetAllBooksAsync();
    Task<BookDto> GetBookByTitleAsync(string title);
    Task<BookDto> CreateBookAsync(BookCreateDto bookDto);
    Task<BookDto> CreateLoanAsync(BookLoanDto bookLoan);
}