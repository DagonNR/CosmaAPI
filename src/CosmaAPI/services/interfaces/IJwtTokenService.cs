using CosmaAPI.auth;
using CosmaAPI.entities;
namespace CosmaAPI.services.interfaces;

public interface IJwtTokenService
{
    AuthTokenResult GenerateToken(User user);
}