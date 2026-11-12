namespace Glitch.Classes
{
    public class Message
    {
        public int ID { get; set; }

        public int SenderProfileID { get; set; }
        
        public int ReceiverProfileID { get; set; }

        public string Text { get; set; }

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }
    }
}
