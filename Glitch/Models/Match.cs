namespace Glitch.Models
{
    public class Match
    {
        public int ID { get; set; }

        public int ProfileAID { get; set; }

        public int ProfileBID { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
