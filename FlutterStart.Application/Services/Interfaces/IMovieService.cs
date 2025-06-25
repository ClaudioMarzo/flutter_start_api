using Microsoft.AspNetCore.Http;
using FlutterStart.Application.DTO.Movie;

namespace FlutterStart.Application.Services.Interfaces;

public interface IMovieService
{
    Task<IEnumerable<MovieReadDto>> GetAllAsync();
    Task<MovieReadDto> CreateMovieAsync(MovieCreateDto dto);
    Task<MovieReadDto> UploadTrailerAsync(int movieId, IFormFile trailerFile);
}