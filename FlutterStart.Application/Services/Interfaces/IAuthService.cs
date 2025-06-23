using FlutterStart.Application.DTOs.User;
using FlutterStart.Domain.Entities;

namespace FlutterStart.Application.Services.Interfaces;
public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterRequest request);
    Task<UserDto?> LoginAsync(LoginRequest request);
}