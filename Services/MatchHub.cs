using Microsoft.AspNetCore.SignalR;

namespace MyField.Services
{
    public class MatchHub : Hub
    {
        public async Task SendUpdate(int fixtureId)
        {
            string groupName = fixtureId.ToString();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("ReceiveUpdate", fixtureId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var fixtureId = Context.GetHttpContext().Request.Query["fixtureId"].FirstOrDefault();
            if (!string.IsNullOrEmpty(fixtureId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, fixtureId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
