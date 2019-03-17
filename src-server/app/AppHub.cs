using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ServiceStack;

namespace app
{
    public class AppHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var user = Context.User;

            Console.WriteLine($"User { user?.Identity.Name ?? "unknown" } connected (cid: {connectionId})");

            await Groups.AddToGroupAsync(Context.ConnectionId, "Users");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var user = Context.User;

            Console.WriteLine($"User { user?.Identity.Name ?? "unknown" } disconnected (cid: {connectionId})");
            //cool

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Users");
            await base.OnDisconnectedAsync(exception);
        }

        public Task SendMessage(HubMessage message)
        {
            switch (message.Audience)
            {
                case HubMessageAudience.Caller:
                    return Clients.Caller.SendAsync("ReceiveMessage", message);
                case HubMessageAudience.Others:
                    return string.IsNullOrWhiteSpace(message.Group) ?
                        Clients.Others.SendAsync("ReceiveMessage", message) :
                        Clients.OthersInGroup(message.Group).SendAsync("ReceiveMessage", message);
                case HubMessageAudience.All:
                default:
                    return string.IsNullOrWhiteSpace(message.Group) ?
                        Clients.All.SendAsync("ReceiveMessage", message) :
                        Clients.Group(message.Group).SendAsync("ReceiveMessage", message);
            }
        }
    }

    public class HubMessage
    {
        public HubMessageAudience? Audience { get; set; }
        public string Group { get; set; }
        public string Action { get; set; }
        public virtual Dictionary<string, object> Data { get; set; }
    }

    public enum HubMessageAudience
    {
        Caller,
        Others,
        All
    }
}