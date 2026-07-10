namespace CommunityReport.Models
{
    public class ChatRoomMember
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        public string UserId { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
