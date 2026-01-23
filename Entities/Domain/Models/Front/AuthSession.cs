namespace Entities.Domain.Models.Front;

public class AuthSession
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !string.IsNullOrEmpty(AccessToken) && !IsExpired;
}