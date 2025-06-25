using Microsoft.AspNetCore.Http;
using FlutterStart.Domain.Entities;
using Microsoft.Extensions.Logging;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Infrastructure.Repository;


public class MovieRepository : IMovieRepository
{
    private readonly ILogger<MovieRepository> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public MovieRepository(ILogger<MovieRepository> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    public Task<Movie> CreateMovieAsync(Movie movie)
    {
        throw new NotImplementedException();
    }

    public Task<List<Movie>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Movie> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Movie> UploadTrailerAsync(Movie movie)
    {
        throw new NotImplementedException();
    }
}