using FlutterStart.Application.DTO;
using FlutterStart.Application.DTO.Book;

namespace FlutterStart.Application.Services.Interfaces;

public interface IBookService
{
    Task<List<BookDto>> GetAllBooksAsync();
    Task<List<BookDto>> GetBookByTitleAsync(string title);
    Task<BookDto> CreateBookAsync(BookCreateDto bookDto);
    Task<LoanResponseDto> CreateLoanAsync(LoanRequestDto bookLoan);
}