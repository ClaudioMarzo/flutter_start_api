using FlutterStart.Domain.Entities;

namespace FlutterStart.Infrastructure.Repository.Interfaces;

public interface IBookRepository
{
    Task<List<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(string ISBN);
    Task<List<Book>> GetBooksByTitleAsync(string title);
    Task<Book> CreateBookAsync(Book bookDto);
    Task<Book> CreateLoanAsync(Loan bookLoan);
}