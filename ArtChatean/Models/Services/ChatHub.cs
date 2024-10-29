using Microsoft.AspNetCore.SignalR;

namespace ArtChatean.Models.Services
{
    public class ChatHub : Hub
    {
        private readonly ArtDbContext _context;

        public ChatHub(ArtDbContext context)
        {
            _context = context;
        }
        public async Task SendMessage(string userId, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", userId, message);
        }
        public async Task SendPrivateMessage(string senderId, string receiverId, string message)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = int.Parse(senderId),
                ReceiverId = int.Parse(receiverId),
                MessageText = message,
                Timestamp = DateTime.Now,
                IsRead = false
            };

            // Додаємо повідомлення до бази даних
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", message, senderId);
        }
        public async Task SendPrivateMessageWithImage(string senderId, string receiverId, string message, string base64Image)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = int.Parse(senderId),
                ReceiverId = int.Parse(receiverId),
                MessageText = message,
                Timestamp = DateTime.Now,
                IsRead = false
            };

            if (!string.IsNullOrEmpty(base64Image))
            {
                chatMessage.MessageImage = Convert.FromBase64String(base64Image); // Конвертуємо з base64 у byte[]
            }

            // Додаємо повідомлення до бази даних
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Відправляємо повідомлення користувачу
            await Clients.User(receiverId).SendAsync("ReceivePrivateMessageWithImage", message, senderId, base64Image);
        }
    }
}

