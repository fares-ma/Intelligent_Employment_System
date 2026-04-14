using System;

namespace Domain.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? Details { get; set; } // JSON or string payload representing extra info
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }
    }
}
