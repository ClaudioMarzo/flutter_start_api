using FlutterStart.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Infrastructure.Repository;


public class MovieRepository : IMovieRepository
{
    private readonly FlutterStartDbContext _context;
    private readonly ILogger<MovieRepository> _logger;
    public MovieRepository(ILogger<MovieRepository> logger, FlutterStartDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public Task<Movie> CreateMovieAsync(Movie movie)
    {
        _context.Movies.Add(movie);
        _context.SaveChanges();
        return Task.FromResult(movie);
    }

    public Task<List<Movie>> GetAllAsync()
    {
        var movies = _context.Movies.AsNoTracking().OrderBy(m => m.Id).ToList();
        return Task.FromResult(movies);
    }

    public Task<Movie> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Movie?> GetMovieByIMDB(string IMDB)
    {
        var movie = _context.Movies.AsNoTracking().FirstOrDefault(m => m.IMDB == IMDB);
        return Task.FromResult(movie);
    }

    public Task<Movie> UploadTrailerAsync(Movie movie)
    {
        throw new NotImplementedException();
    }
}