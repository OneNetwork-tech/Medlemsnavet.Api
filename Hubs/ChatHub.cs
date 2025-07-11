using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Medlemsnavet.Hubs;

// The Hub is the server-side component that receives and sends messages.
public class ChatHub : Hub
{
    // This method can be called by a connected client to send a message.
    public async Task SendMessage(string user, string message)
    {
        // This sends the received message to ALL connected clients.
        // The clients will listen for the "ReceiveMessage" event.
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}