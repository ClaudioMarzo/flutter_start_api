using FlutterStart.Domain.Entities;

namespace FlutterStart.Infrastructure.Repository.Interfaces;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<User> GetUserEmailIdAsync(int userId);
    Task<bool> IsUserExistsAsync(string email);
    Task<bool> CheckPasswordAsync(string senhaHash, string senha);
}