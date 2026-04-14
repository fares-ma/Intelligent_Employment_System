using System;

namespace Services.Abstractions.DTOs.ActivityLog
{
    public class ActivityLogDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
