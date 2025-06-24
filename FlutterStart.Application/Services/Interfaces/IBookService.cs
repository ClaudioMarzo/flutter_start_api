using FlutterStart.Application.DTO;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Domain.Entities;

namespace FlutterStart.Application.Services.Interfaces;

public interface IBookService
{
    Task<List<BookDto>> GetAllBooksAsync();
    Task<List<BookDto>> GetBookByTitleAsync(string title);
    Task<BookDto> CreateBookAsync(BookCreateDto bookDto);
    Task<BookDto> CreateLoanAsync(BookLoanDto bookLoan);
}