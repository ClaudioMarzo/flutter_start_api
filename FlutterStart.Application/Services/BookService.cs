using AutoMapper;
using Microsoft.Extensions.Logging;
using FlutterStart.Application.DTO;
using FlutterStart.Domain.Entities;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Application.Exceptions;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Application.Services;

public class BookService : IBookService
{
    private readonly IMapper _mapper;
    private readonly ILogger<BookService> _logger;
    private readonly IBookRepository _bookRepository;
    private readonly IFileStorageService _fileStorageService;

    public BookService(IMapper mapper, ILogger<BookService> logger, IBookRepository bookRepository, IFileStorageService fileStorageService)
    {
        _mapper = mapper;
        _logger = logger;
        _bookRepository = bookRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<BookDto> CreateBookAsync(BookCreateDto bookDto)
    {
        _logger.LogInformation("Iniciando criação de livro: {Title}", bookDto.Title);
        try
        {
            var bookExist = await _bookRepository.GetBookByIdAsync(bookDto.Isbn!);
            if (bookExist != null)
                throw new ConflictException($"Livro com {bookDto.Isbn} já existe");
            
            var pathImage = await _fileStorageService.SaveImageAsync(bookDto.ImageUrl!, "books");
            if (string.IsNullOrEmpty(pathImage))
                throw new InvalidOperationException("Erro ao salvar a imagem do livro. Verifique o arquivo enviado.");

            Book book = new()
            {
                ISBN = bookDto.Isbn,
                Title = bookDto.Title,
                Summary = bookDto.Summary,
                Genre = bookDto.Genre,
                Author = bookDto.Author,
                PublicationYear = bookDto.PublicationYear,
                PageCount = bookDto.PageCount,
                Publisher = bookDto.Publisher,
                Edition = bookDto.Edition,
                ImageUrl = pathImage,
                Language = bookDto.Language,
                Format = bookDto.Format,
                Dimensions = bookDto.Dimensions,
                Location = bookDto.Location,
                IsRented = false
            };
            var createdBook = await _bookRepository.CreateBookAsync(book);

            _logger.LogInformation("Livro criado com sucesso: {Title}", createdBook.Title);
            return _mapper.Map<BookDto>(createdBook);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar livro");
            throw new Exception("Erro interno do servidor", ex);
        }
    }

    public Task<BookDto> CreateLoanAsync(BookLoanDto bookLoan)
    {
        throw new NotImplementedException();
    }

    public async Task<List<BookDto>> GetAllBooksAsync()
    {
        try
        {
            var books = await _bookRepository.GetAllBooksAsync();
            if (books == null || !books.Any())
            {
                _logger.LogWarning("Nenhum livro encontrado");
                throw new NotFoundException("Nenhum livro encontrado");
            }
            _logger.LogInformation("Total de livros encontrados: {Count}", books.Count());
            return _mapper.Map<List<BookDto>>(books);
        }
        catch (NotFoundException)
        {
            throw; 
        }
    }

    public async Task<List<BookDto>> GetBookByTitleAsync(string title)
    {
        _logger.LogInformation("Buscando livro por título: {Title}", title);
        try
        {
            var books = await _bookRepository.GetBooksByTitleAsync(title);
            if (books == null || !books.Any())
            {
                _logger.LogWarning("Nenhum livro encontrado: {Title}", title);
                throw new NotFoundException($"Nenhum livro encontrado com título '{title}'");
            }
            return _mapper.Map<List<BookDto>>(books);
        }
        catch (NotFoundException)
        {
            throw; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar livro por título: {Title}", title);
            throw new Exception("Erro interno do servidor", ex);
        }
    }
}