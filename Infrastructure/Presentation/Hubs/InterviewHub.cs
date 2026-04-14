using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Presentation.Hubs;

[Authorize]
public class InterviewHub : Hub
{
    public async Task JoinInterviewRoom(string interviewId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"interview_{interviewId}");
    }
    
    public async Task LeaveInterviewRoom(string interviewId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"interview_{interviewId}");
    }
}
