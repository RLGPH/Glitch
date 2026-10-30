namespace Glitch.Models
{
    public class User
    {
        public int UserID { get; set; }

        public DateTime BirthDate { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public Profile Profile { get; set; }
    }
}
