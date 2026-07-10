namespace CommunityReport.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
    }
}
