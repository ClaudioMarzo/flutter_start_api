using FlutterStart.Domain.Entities;

namespace FlutterStart.Infrastructure.Repository.Interfaces;

public interface IAuthRepository
{
    Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(string senha, string senhaHash);
}