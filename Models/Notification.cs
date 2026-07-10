namespace CommunityReport.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "Complaint", "System", "General"
        public int? ComplaintId { get; set; } // Link to complaint if applicable
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
