using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace POS.Api.Hubs
{
    [Authorize]
    public class SalesHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var branchIdClaim = Context.User?.FindFirst("branchId")?.Value;
            if (!string.IsNullOrEmpty(branchIdClaim))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"branch-{branchIdClaim}");
                // In production, consider real logging instead of console
                Console.WriteLine($"Client connected to branch-{branchIdClaim}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var branchIdClaim = Context.User?.FindFirst("branchId")?.Value;
            if (!string.IsNullOrEmpty(branchIdClaim))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch-{branchIdClaim}");
                Console.WriteLine($"Client disconnected from branch-{branchIdClaim}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinBranch(int branchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"branch-{branchId}");
        }
    }
}
