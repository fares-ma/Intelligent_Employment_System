using System;

namespace Domain.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        // Context
        public int? JobApplicationId { get; set; }

        public ApplicationUser? Sender { get; set; }
        public ApplicationUser? Receiver { get; set; }
        public JobApplication? JobApplication { get; set; }
    }
}
