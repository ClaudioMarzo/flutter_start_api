using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using FlutterStart.Domain.Entities;
using Microsoft.Extensions.Hosting;
using FlutterStart.Infrastructure.DTO;
using FlutterStart.Application.DTO.Movie;
using FlutterStart.Application.Exceptions;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Infrastructure.Services.Interfaces;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MovieService> _logger;
    private readonly IMovieRepository _movieRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IFileStorageService _fileStorageService;

    public MovieService(IWebHostEnvironment env, IMapper mapper, ILogger<MovieService> logger, IMovieRepository movieRepository, ICloudinaryService cloudinaryService, IFileStorageService fileStorageService)
    {
        _env = env;
        _mapper = mapper;
        _logger = logger;
        _movieRepository = movieRepository;
        _cloudinaryService = cloudinaryService;
        _fileStorageService = fileStorageService;
    }

    public async Task<MovieResponseDto> CreateMovieAsync(MovieCreateDto dto)
    {
        try
        {
            var movieExist = await _movieRepository.GetMovieByIMDB(dto.IMDB!);
            if (movieExist != null)
                throw new ConflictException("Filme já cadastrado");

            MovieUploadResultDto uploadResult;

            if (_env.IsDevelopment())
            {
                _logger.LogInformation("Ambiente de desenvolvimento detectado. Salvando imagem localmente.");
                var localPath = await _fileStorageService.SaveImageAsync(dto.Poster!, "movies");
                uploadResult = new MovieUploadResultDto { Url = localPath, PublicId = string.Empty };
            }
            else
            {
                _logger.LogInformation("Ambiente de produção detectado. Enviando imagem para o armazenamento.");
                var imageUploadResult = await _cloudinaryService.UploadImageAsync(dto.Poster!, "movies");
                uploadResult = new MovieUploadResultDto { Url = imageUploadResult.Url, PublicId = string.Empty };
            }

            if (string.IsNullOrEmpty(uploadResult.Url))
                throw new InvalidOperationException("Erro ao salvar a imagem do filme. Verifique o arquivo enviado.");

            Movie movie = new()
            {
                IMDB = dto.IMDB,
                Title = dto.Title,
                Description = dto.Description,
                Year = dto.Year,
                Language = dto.Language,
                DurationMinutes = dto.DurationMinutes,
                Genre = dto.Genre,
                Director = dto.Director,
                Cast = dto.Cast,
                PosterUrl = uploadResult.Url,
                TrailerUrl = string.Empty
            };
            
            _logger.LogInformation("Criando filme: {Title}", movie.Title);
            var movieCreate = await _movieRepository.CreateMovieAsync(movie);

            return _mapper.Map<MovieResponseDto>(movieCreate);
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar filme");
            throw new ArgumentException("Erro ao criar filme", ex);
        }
    }

    public async Task<List<MovieResponseDto>> GetAllAsync()
    {
        try
        {
            var movies = await _movieRepository.GetAllAsync();
            if (movies == null || !movies.Any())
                throw new NotFoundException("Nenhum filme encontrado");

            _logger.LogInformation("Obtendo {Count} filmes", movies.Count);
            return _mapper.Map<List<MovieResponseDto>>(movies);
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
    public async Task<MovieResponseDto> UploadTrailerAsync(MovieUpdateTrailerDto updateTrailerDto)
    {
        try
        {
            bool isPostImageEmpty = updateTrailerDto.PostImage == null || updateTrailerDto.PostImage.Length == 0;
            bool isTrailerMP4Empty = updateTrailerDto.TrailerMP4 == null || updateTrailerDto.TrailerMP4.Length == 0;
            if (isPostImageEmpty && isTrailerMP4Empty)
            {
                _logger.LogWarning("Nenhum arquivo enviado para atualização do trailer");
                throw new ArgumentException("Nenhum arquivo enviado para atualização do trailer");
            }

            var movieExist = await _movieRepository.GetByIdAsync(updateTrailerDto.Id)
                ?? throw new NotFoundException("Filme não encontrado");

            // Atualiza imagem se enviada
            if (!isPostImageEmpty)
            {
                try
                {
                    var imageUploadResult = await _cloudinaryService.UploadImageAsync(updateTrailerDto.PostImage!, "movies");
                    movieExist.PosterUrl = imageUploadResult.Url;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar a imagem do filme");
                    throw new ArgumentException("Erro ao processar a imagem do filme", ex);
                }
            }

            // Atualiza trailer se enviado
            if (!isTrailerMP4Empty)
            {
                try
                {
                    var videoUploadResult = await _cloudinaryService.UploadVideoAsync(updateTrailerDto.TrailerMP4!, "movies");
                    if (string.IsNullOrEmpty(videoUploadResult.Url))
                        throw new InvalidOperationException("Erro ao salvar o trailer do filme. Verifique o arquivo enviado.");
                    movieExist.TrailerUrl = videoUploadResult.Url;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar o trailer do filme");
                    throw new ArgumentException("Erro ao processar o trailer do filme", ex);
                }
            }

            movieExist.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Atualizando trailer do filme: {Title}", movieExist.Title);
            var updatedMovie = await _movieRepository.UploadTrailerAsync(movieExist)
                ?? throw new InvalidOperationException("Erro ao atualizar trailer do filme");

            _logger.LogInformation("Trailer atualizado com sucesso para o filme: {Title}", updatedMovie.Title);
            return _mapper.Map<MovieResponseDto>(updatedMovie);
        }
        catch (ArgumentException) { throw; }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao atualizar trailer do filme");
            throw new InvalidOperationException("Erro inesperado ao atualizar trailer do filme", ex);
        }
    }
    
    public async Task<List<MovieResponseDto>> GetMovieByTitleAsync(string title)
    {
        _logger.LogInformation("Buscando livro por título: {Title}", title);
        try
        {
            var movies = await _movieRepository.GetMovieByTitleAsync(title);
            if (movies == null || !movies.Any())
            {
                _logger.LogWarning("Nenhum filme encontrado: {Title}", title);
                throw new NotFoundException($"Nenhum filme encontrado com título '{title}'");
            }
            return _mapper.Map<List<MovieResponseDto>>(movies);
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