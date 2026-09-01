using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
namespace Sym_Connect__Capstone2_.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                if (Context.User.IsInRole("Admin"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId,
                            "Admins");
                }
            }
            await base.OnConnectedAsync();

        }

    }
}
