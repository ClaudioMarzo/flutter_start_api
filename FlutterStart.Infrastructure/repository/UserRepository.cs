using FlutterStart.Domain.Entities;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Infrastructure.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlutterStart.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly FlutterStartDbContext _context;

    public UserRepository(ILogger<UserRepository> logger, FlutterStartDbContext context)
    {
        _context = context;
        _logger = logger;
    }
    public Task<bool> CheckPasswordAsync(string senhaHash, string senha)
    {
        throw new NotImplementedException();
    }

    public Task<User> CreateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserEmailIdAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserExistsAsync(string email)
    {
        throw new NotImplementedException();
    }
}