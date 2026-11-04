namespace Glitch.Classes
{
    public class Swipe
    {
        public int ID { get; set; }

        public int FromPrifileID { get; set; }

        public int ToProfileID { get; set; }

        public bool IsLiked { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
