using Microsoft.AspNetCore.Mvc;
using FlutterStart.Application.DTO.Movie;
using FlutterStart.Application.Services.Interfaces;

namespace FlutterStart.Presentation.Controller;

[ApiController]
[Route("[controller]")]
public class MovieController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MovieController> _logger;

    public MovieController(IMovieService movieService, ILogger<MovieController> logger)
    {
        _logger = logger;
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMovie()
    {
        var movies = await _movieService.GetAllAsync();
        return Ok(movies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movies = await _movieService.GetAllAsync();
        var movie = movies.FirstOrDefault(m => m.Id == id);
        if (movie == null) return NotFound();
        return Ok(movie);
    }

    [HttpPost("movies-create")]
    public async Task<IActionResult> Post([FromForm] MovieCreateDto dto)
    {
        var movie = await _movieService.CreateMovieAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPost("upload-trailer/{id}")]
    public async Task<IActionResult> UploadTrailer(int id, [FromForm] IFormFile trailer)
    {
        try
        {
            var trailerUrl = await _movieService.UploadTrailerAsync(id, trailer);
            return Ok(new { TrailerUrl = trailerUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}