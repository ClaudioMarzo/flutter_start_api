using AutoMapper;
using Microsoft.Extensions.Logging;
using FlutterStart.Application.DTO;
using FlutterStart.Domain.Entities;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Application.Exceptions;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Application.Services;

public class BookService : IBookService
{
    private readonly IMapper _mapper;
    private readonly ILogger<BookService> _logger;
    private readonly FlutterStartDbContext _context;
    private readonly IBookRepository _bookRepository;
    private readonly IFileStorageService _fileStorageService;

    public BookService(IMapper mapper, FlutterStartDbContext context, ILogger<BookService> logger, IBookRepository bookRepository, IFileStorageService fileStorageService)
    {
        _mapper = mapper;
        _logger = logger;
        _context = context;
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

    public Task<IEnumerable<BookResponseDto>> GetAllBooksAsync()
    {
        throw new NotImplementedException();
    }

    public Task<BookDto> GetBookByTitleAsync(string title)
    {
        throw new NotImplementedException();
    }
}