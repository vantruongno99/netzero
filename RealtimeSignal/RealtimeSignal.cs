using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RealtimeSignal
{
    public class RealtimeSignalHub : Hub
    { 

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

    }
 
    
}