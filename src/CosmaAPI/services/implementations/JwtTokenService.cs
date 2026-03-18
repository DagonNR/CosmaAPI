using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CosmaAPI.auth;
using CosmaAPI.entities;
using CosmaAPI.services.interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace CosmaAPI.services.implementations;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public AuthTokenResult GenerateToken(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Key)
        );
        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );
        var jwtToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );
        var token = new JwtSecurityTokenHandler(). WriteToken(jwtToken);

        return new AuthTokenResult
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
