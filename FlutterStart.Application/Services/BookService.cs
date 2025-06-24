using FlutterStart.Application.DTO;
using Microsoft.Extensions.Logging;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Application.Services.Interfaces;

namespace FlutterStart.Application.Services;

public class BookService : IBookService
{
    private readonly ILogger<BookService> _logger;
    private readonly FlutterStartDbContext _context;

    public BookService(FlutterStartDbContext context, ILogger<BookService> logger)
    {
        _logger = logger;
        _context = context;
    }

    public Task<BookDto> CreateBookAsync(BookCreateDto bookDto)
    {
        throw new NotImplementedException();
    }

    public Task<BookDto> CreateLoanAsync(BookLoanDto bookLoan)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BookResponseDto>> GetAllBooksAsync()
    {
        throw new NotImplementedException();
    }

    public Task<BookDto> GetBookByTitleAsync(string title)
    {
        throw new NotImplementedException();
    }
}