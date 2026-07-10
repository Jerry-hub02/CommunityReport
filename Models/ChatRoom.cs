namespace CommunityReport.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
