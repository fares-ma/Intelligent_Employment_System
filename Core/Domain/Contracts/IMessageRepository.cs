using Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IMessageRepository : IRepositoryBase<Message>
    {
        Task<IEnumerable<Message>> GetUserMessagesAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Message>> GetConversationAsync(string userId1, string userId2, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    }
}
