using Microsoft.AspNetCore.Mvc;
using FlutterStart.Application.DTO;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Application.Exceptions;
using FlutterStart.Application.Services.Interfaces;

namespace FlutterStart.Apresentation.Controller;

[ApiController]
[Route("[controller]")]
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
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(typeof(List<BookDto>), 200)]
    [Produces("application/json")]
    public async Task<IActionResult> GetBooks()
    {
        try
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Nenhum livro encontrado");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter livros");
            return StatusCode(500, "Erro interno do servidor");

        }
    }

    [HttpGet("books/{title}")]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(typeof(BookDto), 200)]
    [Produces("application/json")]
    public async Task<IActionResult> GetBookById(string title)
    {
        _logger.LogInformation("Buscando livro por título: {Title}", title);
        try
        {
            var book = await _bookService.GetBookByTitleAsync(title);
            return Ok(book);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Livro não encontrado");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter livro");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("create-book")]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    [Produces("application/json")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BookDto), 201)]
    public async Task<IActionResult> CreateBook([FromForm] BookCreateDto bookDto)
    {
        _logger.LogInformation("Iniciando criação de livro: {Title}", bookDto.Title);
        try
        {
            var createdBook = await _bookService.CreateBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { title = createdBook.Title }, createdBook);
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
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(typeof(LoanResponseDto), 201)]
    [Produces("application/json")]
    public async Task<IActionResult> CreateLoan([FromBody] LoanRequestDto bookLoan)
    {
        _logger.LogInformation("Iniciando criação de empréstimo para LivroId: {LivroId}", bookLoan.BookId);
        try
        {
            var loan = await _bookService.CreateLoanAsync(bookLoan);
            return CreatedAtAction(nameof(GetBookById), new { title = loan.BookTitle }, loan);
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