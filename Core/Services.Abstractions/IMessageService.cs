using Services.Abstractions.DTOs.Message;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IMessageService
    {
        Task<MessageDto> SendMessageAsync(string senderId, SendMessageRequest request);
        Task<IEnumerable<ConversationSummaryDto>> GetUserConversationsAsync(string userId);
        Task<IEnumerable<MessageDto>> GetConversationAsync(string userId1, string userId2);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int messageId, string userId);
    }
}
