namespace Entities.Contracts.Forms;

public class RegisterForm
{
    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public string ConfirmPassword { get; set; } = String.Empty;
    public string Username { get; set; } = String.Empty;
}