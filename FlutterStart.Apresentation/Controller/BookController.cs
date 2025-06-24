using Microsoft.AspNetCore.Mvc;
using FlutterStart.Application.DTO;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Application.Services.Interfaces;

namespace FlutterStart.Apresentation.Controller;

public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BookController> _logger;

    public BookController(IBookService bookService, ILogger<BookController> logger)
    {
        _logger = logger;
        _bookService = bookService;
    }

    [HttpGet("books")]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpGet("books/{title}")]
    public async Task<IActionResult> GetBookById(string title)
    {
        var book = await _bookService.GetBookByTitleAsync(title);
        if (book == null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    [HttpPost("create-book")]
    public async Task<IActionResult> CreateBook([FromBody] BookCreateDto bookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdBook = await _bookService.CreateBookAsync(bookDto);
        return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
    }
    [HttpPost("create-loan")]
    public async Task<IActionResult> CreateLoan([FromBody] BookLoanDto bookLoan)
    {

        var loan = await _bookService.CreateLoanAsync(bookLoan);
        if (loan == null)
        {
            return NotFound();
        }
        
        return CreatedAtAction(nameof(GetBookById), new { title = loan.Title }, loan);
    }
}