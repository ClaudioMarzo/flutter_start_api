using Microsoft.AspNetCore.Mvc;
using FlutterStart.Application.DTO;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Application.Exceptions;

namespace FlutterStart.Apresentation.Controller;
[ApiController]
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
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    [Produces("application/json")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BookDto), 201)]
    public async Task<IActionResult> CreateBook([FromBody] BookCreateDto bookDto)
    {
        try
        {
            var createdBook = await _bookService.CreateBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);

        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para criação do livro");
            return BadRequest(ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Livro já existe");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar livro");
            return StatusCode(500, "Erro interno do servidor");
        }
    }
    [HttpPost("create-loan")]
    public async Task<IActionResult> CreateLoan([FromBody] BookLoanDto bookLoan)
    {
        try
        {
            var loan = await _bookService.CreateLoanAsync(bookLoan);
            if (loan == null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetBookById), new { title = loan.Title }, loan);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Já existe um empréstimo ativo para este livro");
            return NotFound(ex.Message);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating loan");
            return StatusCode(500, "Internal server error");
        }
    }
}