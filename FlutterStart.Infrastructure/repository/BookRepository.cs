using FlutterStart.Domain.Entities;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Infrastructure.Repository;

public class BookRepository : IBookRepository
{
    private readonly FlutterStartDbContext _context;

    public BookRepository(FlutterStartDbContext context)
    {
        _context = context;
    }

    public Task<Book> CreateBookAsync(Book bookDto)
    {
        throw new NotImplementedException();
    }

    public Task<Book> CreateLoanAsync(Loan bookLoan)
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<Book>> IBookRepository.GetAllBooksAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Book> GetBookByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    Task<Book> IBookRepository.GetBookByTitleAsync(string title)
    {
        throw new NotImplementedException();
    }
}