using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CommunityReport.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int chatRoomId, string username, string message)
        {
            Console.WriteLine($"SendMessage called: Room={chatRoomId}, User={username}, Message={message}");

            // Send to ALL clients in the group
            await Clients.Group(chatRoomId.ToString())
                .SendAsync("ReceiveMessage", username, message, DateTime.Now.ToString("hh:mm tt"));
        }

        public async Task JoinRoom(int chatRoomId)
        {
            Console.WriteLine($"JoinRoom called: Room={chatRoomId}, ConnectionId={Context.ConnectionId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());

            Console.WriteLine($"User joined room {chatRoomId}");
        }

        public async Task LeaveRoom(int chatRoomId)
        {
            Console.WriteLine($"LeaveRoom called: Room={chatRoomId}");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
        }
    }
}
