using Microsoft.AspNetCore.Http;
using FlutterStart.Application.DTO.Movie;

namespace FlutterStart.Application.Services.Interfaces;

public interface IMovieService
{
    Task<List<MovieResponseDto>> GetAllAsync();
    Task<MovieResponseDto> CreateMovieAsync(MovieCreateDto dto);
    Task<MovieResponseDto> UploadTrailerAsync(MovieUpdateTrailerDto updateTrailerDto);
}