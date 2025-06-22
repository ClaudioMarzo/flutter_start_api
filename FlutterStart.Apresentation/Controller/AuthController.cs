using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Application.DTOs.User;

namespace ViberLounge.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Recebendo requisição de login para o usuário {Email}", request.Email!);
        try
        {
            var result = await _authService.LoginAsync(request);
            _logger.LogInformation("Login realizado com sucesso para o usuário {Email}", request.Email!);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha de autenticação: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Recebendo requisição de registro");
        try
        {
            var newUser = await _authService.RegisterAsync(request);
            _logger.LogInformation("Usuário {Email} registrado com sucesso", newUser.Email!);
            return Created(string.Empty, new { message = "Usuário cadastrado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário");
            return BadRequest(new { message = ex.Message });
        }
    }
}