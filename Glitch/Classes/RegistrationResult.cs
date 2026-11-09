namespace Glitch.Classes;

public class RegistrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? CreatedUser { get; set; }
}