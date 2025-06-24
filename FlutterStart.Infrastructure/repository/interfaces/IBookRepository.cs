using FlutterStart.Domain.Entities;

namespace FlutterStart.Infrastructure.Repository.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book> GetBookByIdAsync(string ISBN);
    Task<Book> GetBookByTitleAsync(string title);
    Task<Book> CreateBookAsync(Book bookDto);
    Task<Book> CreateLoanAsync(Loan bookLoan);
}