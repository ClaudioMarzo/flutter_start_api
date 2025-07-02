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

    public Task<Movie?> GetByIdAsync(int id)
    {
        return _context.Movies.AsTracking().FirstOrDefaultAsync(m => m.Id == id);
    }

    public Task<Movie?> GetMovieByIMDB(string IMDB)
    {
        return _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.IMDB == IMDB);
    }

    public async Task<List<Movie>> GetMovieByTitleAsync(string title)
    {
        var sql = @"SELECT * FROM Movies WHERE 
                similarity(unaccent(Title), unaccent({0})) > 0.3 
                ORDER BY similarity(unaccent(Title), unaccent({0})) DESC";
        var movies = await _context.Movies
            .FromSqlRaw(sql, title)
            .AsNoTracking()
            .ToListAsync();
        return movies;
    }

    public async Task<Movie> UploadTrailerAsync(Movie movie)
    {
        await _context.SaveChangesAsync();
        return movie;
    }
}