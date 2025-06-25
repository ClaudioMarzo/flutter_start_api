using FlutterStart.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Infrastructure.Repository;

public class BookRepository : IBookRepository
{
    private readonly FlutterStartDbContext _context;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(FlutterStartDbContext context, ILogger<BookRepository> logger)
    {
        _logger = logger;
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

    public async Task<Loan?> CreateLoanAsync(Loan bookLoan, Book book)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Loans.Add(bookLoan);
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var bookResponse = await _context.Loans
                .Include(b => b.Book)
                .Include(u => u.User)
                .AsNoTracking()
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync(l => l.BookId == book.Id);

            return bookResponse;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Erro ao criar o empréstimo do livro com ID {BookId}", book.Id);
            await transaction.RollbackAsync();
            return null;
        }
    }

    public Task<Book?> GetBookByISBNAsync(string ISBN)
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

    public Task<Book?> GetBookByIdAsync(int id)
    {
        var book = _context.Books.AsTracking().FirstOrDefault(b => b.Id == id);
        return Task.FromResult(book);
    }
}