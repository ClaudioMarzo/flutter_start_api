using FlutterStart.Domain.Entities;

namespace FlutterStart.Infrastructure.Repository.Interfaces;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllAsync();
    Task<Movie?> GetMovieByIMDB(string IMDB);
    Task<Movie> GetByIdAsync(int id);
    Task<Movie> CreateMovieAsync(Movie movie);
    Task<Movie> UploadTrailerAsync(Movie movie);
}