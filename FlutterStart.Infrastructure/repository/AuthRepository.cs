using FlutterStart.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Infrastructure.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly ILogger<AuthRepository> _logger;
    private readonly FlutterStartDbContext _context;

    public AuthRepository(ILogger<AuthRepository> logger, FlutterStartDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public Task<bool> CheckPasswordAsync(string senha, string senhaHash)
    {
        bool isValid = BCrypt.Net.BCrypt.Verify(senha, senhaHash);
        return Task.FromResult(isValid);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var isUserExist = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        return isUserExist;
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }
}