using CosmaAPI.data;
using CosmaAPI.DTOs.auth;
using CosmaAPI.entities;
using CosmaAPI.services.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CosmaAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IPasswordHasher<User> passwordHasher
    )
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();
        var emailExists = await _dbContext.Users
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        
        if (emailExists)
        {
            throw new InvalidOperationException("El email ya está registrado.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Email = normalizedEmail,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tokenResult = _jwtTokenService.GenerateToken(user);
        
        return new AuthResponseDto
        {
            Token = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("Email o contraseña invalidos");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException("Email o contraseña invalidos");
        }

        var tokenResult = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => new CurrentUserDto
            {
                UserId = x.Id,
                Name = x.Name,
                Email = x.Email
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}