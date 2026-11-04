namespace Glitch.Models
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Profile UserProfile { get; set; }
    }
}
