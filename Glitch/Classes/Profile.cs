namespace Glitch.Models
{
    public class Profile 
    {
        public int ProfileID { get; set; }

        public int UserID { get; set; }

        public string DisplayName { get; set; }

        public string Bio { get; set; }

        public string ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
