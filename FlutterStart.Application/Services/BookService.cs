using AutoMapper;
using Microsoft.Extensions.Logging;
using FlutterStart.Application.DTO;
using FlutterStart.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using FlutterStart.Infrastructure.DTO;
using FlutterStart.Application.DTO.Book;
using FlutterStart.Application.Exceptions;
using static FlutterStart.Domain.Entities.Loan;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Infrastructure.Services.Interfaces;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Application.Services;

public class BookService : IBookService
{
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BookService> _logger;
    private readonly IBookRepository _bookRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICloudinaryService _imageStorageService;

    public BookService(IMapper mapper, IWebHostEnvironment env, ILogger<BookService> logger, IBookRepository bookRepository, IAuthRepository authRepository, IFileStorageService fileStorageService, ICloudinaryService imageStorageService)
    {
        _env = env;
        _mapper = mapper;
        _logger = logger;
        _bookRepository = bookRepository;
        _authRepository = authRepository;
        _fileStorageService = fileStorageService;
        _imageStorageService = imageStorageService;
    }

    public async Task<BookDto> CreateBookAsync(BookCreateDto bookDto)
    {
        _logger.LogInformation("Iniciando criação de livro: {Title}", bookDto.Title);
        try
        {
            var bookExist = await _bookRepository.GetBookByISBNAsync(bookDto.Isbn!);
            if (bookExist != null)
                throw new ConflictException($"Livro com {bookDto.Isbn} já existe");

            ImageUploadResultDto uploadResult;

            if (_env.IsDevelopment())
            {
                _logger.LogInformation("Ambiente de desenvolvimento detectado. Salvando imagem localmente.");
                var localPath = await _fileStorageService.SaveImageAsync(bookDto.ImageUrl!, "books");
                uploadResult = new ImageUploadResultDto { Url = localPath, PublicId = string.Empty };
            }
            else
            {
                _logger.LogInformation("Ambiente de produção detectado. Enviando imagem para o armazenamento.");
                uploadResult = await _imageStorageService.UploadImageAsync(bookDto.ImageUrl!, "books");
            }

            if (string.IsNullOrEmpty(uploadResult.Url))
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
                ImageUrl = uploadResult.Url,
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

    public async Task<LoanResponseDto> CreateLoanAsync(LoanRequestDto bookLoan)
    {
        try
        {
            var user = await _authRepository.GetUserByIdAsync(bookLoan.UserId);
            if (user == null)
                throw new NotFoundException($"Usuário com ID {bookLoan.UserId} não encontrado");

            var book = await _bookRepository.GetBookByIdAsync(bookLoan.BookId);
            if (book == null)
                throw new NotFoundException($"Livro com ID {bookLoan.BookId} não encontrado");
            if (book.IsRented)
                throw new ConflictException($"Livro com ID {bookLoan.BookId} já está emprestado");

            Loan loan = new()
            {
                BookId = bookLoan.BookId,
                UserId = bookLoan.UserId,
                Status = LoanStatus.Borrowed,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddMonths(1), // Definindo prazo de devolução para 1 mês
                Observations = bookLoan.Observations ?? string.Empty
            };
            book.IsRented = true;

            _logger.LogInformation("Iniciando criação de empréstimo para LivroId: {LivroId}, UsuárioId: {UsuarioId}", bookLoan.BookId, bookLoan.UserId);
            var createdLoan = await _bookRepository.CreateLoanAsync(loan, book);

            if (createdLoan == null)
                throw new Exception($"Ocorreu um erro ao criar empréstimo");
            
            return _mapper.Map<LoanResponseDto>(createdLoan);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
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