namespace CosmaAPI.DTOs.auth;

public class CurrentUserDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}