using FlutterStart.Domain.Entities;
using Microsoft.Extensions.Logging;
using FlutterStart.Application.DTOs.User;
using FlutterStart.Application.Services.Interfaces;

namespace FlutterStart.Application.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
    }
    public Task<User> RegisterAsync(RegisterRequest request)
    {
        // Implement registration logic here
        throw new NotImplementedException();
    }

    public Task<UserDto> LoginAsync(LoginRequest request)
    {
        // Implement login logic here
        throw new NotImplementedException();
    }
}