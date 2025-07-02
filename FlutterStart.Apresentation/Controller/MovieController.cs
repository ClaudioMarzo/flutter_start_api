using Microsoft.AspNetCore.Mvc;
using FlutterStart.Application.DTO.Movie;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Application.Exceptions;

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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMovie([FromForm] MovieCreateDto dto)
    {
        _logger.LogInformation("Criando filme: {Title}", dto.Title);
        try
        {
            var movie = await _movieService.CreateMovieAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflito ao criar filme");
            return Conflict(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Filme não encontrado");
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido ao criar filme");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar filme");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("upload-trailer")]
    public async Task<IActionResult> UploadTrailer([FromForm] MovieUpdateTrailerDto trailer)
    {
        try
        {
            var trailerUrl = await _movieService.UploadTrailerAsync(trailer);
            return Ok(new { TrailerUrl = trailerUrl });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}