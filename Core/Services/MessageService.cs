using Domain.Contracts;
using Domain.Models;
using Services.Abstractions;
using Services.Abstractions.DTOs.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MessageDto> SendMessageAsync(string senderId, SendMessageRequest request)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                JobApplicationId = request.JobApplicationId,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _unitOfWork.Messages.Create(message);
            await _unitOfWork.SaveChangesAsync();

            return await MapToDtoAsync(message);
        }

        public async Task<IEnumerable<MessageDto>> GetConversationAsync(string userId1, string userId2)
        {
            var messages = await _unitOfWork.Messages.GetConversationAsync(userId1, userId2);

            var dtos = new List<MessageDto>();
            foreach(var m in messages) 
            {
                dtos.Add(await MapToDtoAsync(m));
            }
            return dtos;
        }

        public async Task<IEnumerable<ConversationSummaryDto>> GetUserConversationsAsync(string userId)
        {
            var messages = await _unitOfWork.Messages.GetUserMessagesAsync(userId);

            var summaries = messages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(group =>
                {
                    var latestMessage = group.OrderByDescending(m => m.SentAt).First();
                    var participant = latestMessage.SenderId == userId ? latestMessage.Receiver : latestMessage.Sender;
                    var participantName = BuildUserDisplayName(participant?.FirstName, participant?.LastName, latestMessage.SenderId == userId ? latestMessage.ReceiverId : latestMessage.SenderId);

                    return new ConversationSummaryDto
                    {
                        ParticipantId = latestMessage.SenderId == userId ? latestMessage.ReceiverId : latestMessage.SenderId,
                        ParticipantName = participantName,
                        LastMessage = latestMessage.Content,
                        LastMessageAt = latestMessage.SentAt,
                        UnreadCount = group.Count(m => m.ReceiverId == userId && !m.IsRead)
                    };
                })
                .OrderByDescending(summary => summary.LastMessageAt)
                .ToList();

            return summaries;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.Messages.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(int messageId, string userId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message != null && message.ReceiverId == userId && !message.IsRead)
            {
                message.IsRead = true;
                _unitOfWork.Messages.Update(message);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task<MessageDto> MapToDtoAsync(Message message)
        {
            var senderName = BuildUserDisplayName(message.Sender?.FirstName, message.Sender?.LastName, message.SenderId);
            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                IsRead = message.IsRead,
                SentAt = message.SentAt,
                JobApplicationId = message.JobApplicationId,
                SenderName = senderName
            };
        }

        private static string BuildUserDisplayName(string? firstName, string? lastName, string fallbackId)
        {
            var fullName = $"{firstName} {lastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? fallbackId : fullName;
        }
    }
}
