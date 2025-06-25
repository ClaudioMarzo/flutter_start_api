using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using FlutterStart.Application.DTO.Movie;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Infrastructure.Services.Interfaces;

namespace FlutterStart.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MovieService> _logger;
    private readonly ICloudinaryService _cloudinaryService;

    public MovieService(IWebHostEnvironment env, IMapper mapper, ILogger<MovieService> logger, ICloudinaryService cloudinaryService)
    {
        _env = env;
        _mapper = mapper;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
    }

    public Task<MovieReadDto> CreateMovieAsync(MovieCreateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<MovieReadDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<MovieReadDto> UploadTrailerAsync(int movieId, IFormFile trailerFile)
    {
        throw new NotImplementedException();
    }
}