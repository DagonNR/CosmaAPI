namespace CosmaAPI.auth;

public class AuthTokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}