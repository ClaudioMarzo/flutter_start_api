using FlutterStart.Domain.Entities;

namespace FlutterStart.Infrastructure.Repository.Interfaces;

public interface IBookRepository
{
    Task<List<Book>> GetAllBooksAsync();
    Task<Book> CreateBookAsync(Book bookDto);
    Task<Loan?> CreateLoanAsync(Loan bookLoan, Book book);
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book?> GetBookByISBNAsync(string ISBN);
    Task<List<Book>> GetBooksByTitleAsync(string title);
}