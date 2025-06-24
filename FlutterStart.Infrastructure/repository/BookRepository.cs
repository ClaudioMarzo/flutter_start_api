using FlutterStart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        if (bookDto == null)
        {
            throw new ArgumentNullException(nameof(bookDto), "Book cannot be null");
        }

        _context.Books.Add(bookDto);
        _context.SaveChanges();
        return Task.FromResult(bookDto);
    }

    public Task<Book> CreateLoanAsync(Loan bookLoan)
    {
        throw new NotImplementedException();
    }

    public Task<Book?> GetBookByIdAsync(string ISBN)
    {
        var book = _context.Books.AsNoTracking().FirstOrDefault(b => b.ISBN == ISBN);
        return Task.FromResult(book);
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _context.Books.AsNoTracking().OrderBy(b => b.Id).ToListAsync();
    }

    public async Task<List<Book>> GetBooksByTitleAsync(string title)
    {
        var books = await _context.Books
            .AsNoTracking()
            .Where(b => EF.Functions.ILike(b.Title!, $"%{title}%"))
            .ToListAsync();
        return books;
    }
}