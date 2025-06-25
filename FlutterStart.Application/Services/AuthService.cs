using FlutterStart.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using FlutterStart.Application.DTOs.User;
using FlutterStart.Application.Services.Interfaces;
using FlutterStart.Infrastructure.Repository.Interfaces;

namespace FlutterStart.Application.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IAuthRepository _authRepository;
    public AuthService(ILogger<AuthService> logger, IAuthRepository authRepository)
    {
        _logger = logger;
        _authRepository = authRepository;
    }
    public async Task<User?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var isEmailExist = await _authRepository.GetUserByEmailAsync(request.Email!);
            if (isEmailExist != null)
            {
                _logger.LogWarning($"Usuário já cadastrado com Email {request.Email}");
                return null;
            }
            if (!ValidatePasswordAndEmail(request.Nome!, request.Email!))
            {
                _logger.LogWarning("Email ou Nome inválido");
                return null;
            }

            string? senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            if (senhaHash == null)
            {
                _logger.LogWarning("Erro ao gerar o hash da senha.");
                return null;
            }
            User user = new()
            {
                Nome = request.Nome,
                Email = request.Email,
                Senha = senhaHash,
                Role = "ADMIN"
            };
            var result = await _authRepository.CreateUserAsync(user);
            
            if (result == null)
            {
                _logger.LogWarning("Erro ao criar usuário na base de dados");
                return null;
            }

            _logger.LogInformation($"Usuário {user.Nome} cadastrado com sucesso");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro inesperado na camada de serviço {ex}");
            return null;
        }
    }

    public async Task<UserDto?> LoginAsync(LoginRequest request)
    {
        try
        {
            var isEmailExist = await _authRepository.GetUserByEmailAsync(request.Email!);
            if (isEmailExist == null)
            {
                _logger.LogWarning("Não há registro desse usuário na base");
                return null;
            }
            if (! await _authRepository.CheckPasswordAsync(request.Senha!, isEmailExist.Senha!))
            {
                _logger.LogWarning("Email ou Senha inválido");
                return null;
            }
  
            UserDto user = new()
            {
                Id = isEmailExist.Id,
                Nome = isEmailExist.Nome,
                Email = isEmailExist.Email,
                Role = isEmailExist.Role
            };

            _logger.LogInformation($"Sucesso ao fazer login para o usuário {isEmailExist.Nome}");
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro inesperado na camada de serviço: {erro}", ex);
            throw;
        }
    }

    private bool ValidatePasswordAndEmail(string nome, string email)
    {
        // Regex para validar nome - aceita nome simples (com pelo menos 3 caracteres) ou nome e sobrenome(s)
        Regex NomeRegex = new Regex(@"^(?:[A-Za-zÀ-ÿ]{3,})+(?:\s+[A-Za-zÀ-ÿ]{2,})*$");
        Regex EmailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@(gmail\.com|googlemail\.com|google\.com)$");
        if (!NomeRegex.IsMatch(nome.Trim()))
        {
            _logger.LogWarning("Nome inválido (deve ter pelo menos 3 letras): {Nome}", nome);
            return false;
        }
        if (!EmailRegex.IsMatch(email.Trim()))
        {
            _logger.LogWarning("Email inválido: {Email}", email);
            return false;
        }
        return true;
    }
}