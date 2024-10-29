using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.SignalR;
using ArtChatean.Models.Services;

namespace ArtChatean.Contollers;

[Authorize]
public class ChatController : Controller
{
    private readonly ArtDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(ArtDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // Метод для відображення чату між користувачами
    [HttpGet]
    public async Task<IActionResult> MainChat(int? receiverId)
    {       
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdstr == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userId = int.Parse(userIdstr);        

        // БЛОК ЯКИЙ ФІЛЬТРУЄ І ГРУПУЄ КОРИСТУВАЧІВ ДЛЯ ОТРИМАННЯ НЕОБХДНИХ ДАННИХ
        
        // Отримуємо ID друзів з останніми повідомленнями
        var friendIdsWithLastMessage = await _context.ChatMessages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    FriendId = g.Key, // ID друга
                    LastMessageTime = g.OrderByDescending(m => m.Timestamp).Select(m => m.Timestamp).FirstOrDefault(), // для побудови списку
                    LastMessage = g.OrderByDescending(m => m.Timestamp).Select(m => m.MessageText).FirstOrDefault() // для додавання під ПІБ користувача
                })
                    .ToListAsync();        

        // Отримуємо друзів за ID
        var friends = await _context.Users
            .Where(u => friendIdsWithLastMessage.Select(f => f.FriendId).Contains(u.Id))
                .Select(user => new
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Image = user.Image
                })
                    .ToListAsync();        

        // Отримуємо кількість непрочитаних повідомлень для кожного друга
        var friendsWithUnreadMessages = await _context.ChatMessages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                    .Select(g => new
                    {
                        FriendId = g.Key,
                        UnreadCount = g.Key == receiverId ? 0 : g.Count() // для відображення кількості непрочитаних повідомлень
                    })
                        .ToListAsync();

        // Обєднуєм у кінцевий список для передачі
        var friendsWithMessages = friends
            .Join(friendIdsWithLastMessage,
                 user => user.Id,
                 msg => msg.FriendId,
                 (user, msg) => new
                 {
                     user.Id,
                     user.FullName,
                     user.Image,
                     LastMessageTime = (DateTime?)msg.LastMessageTime,
                     LastMessage = msg.LastMessage, 
                     UnreadCount = friendsWithUnreadMessages
                         .FirstOrDefault(f => f.FriendId == user.Id)?.UnreadCount ?? 0 // Кількість непрочитаних повідомлень для усіх у списку
                 })
                    .OrderByDescending(f => f.LastMessageTime)
                        .ToList();

        // Блок для можливості написати користувачу, якого немає у чатах, відкриває чат і поміщає його на верх списка
        var isReceiverInList = friendsWithMessages.Any(f => f.Id == receiverId);       
        if (!isReceiverInList)
        {            
            var receiver = await _context.Users
                .Where(u => u.Id == receiverId)
                .Select(user => new
                {
                    user.Id,
                    user.FullName,
                    user.Image
                })
                .FirstOrDefaultAsync();

            if (receiver != null)
            {
                var newFriendWithMessage = new
                {
                    receiver.Id,
                    receiver.FullName,
                    receiver.Image,
                    LastMessageTime = (DateTime?)null,
                    LastMessage = "No messages yet",
                    UnreadCount = 0
                };
                friendsWithMessages.Insert(0, newFriendWithMessage);
            }
        }

        // Передача списка користувачів, з яким спілкувався Юзер
        ViewBag.Friends = friendsWithMessages;

        // Блок для завантаження чату першого користувача у чаті
        if (!receiverId.HasValue)
        {
            var firstFriend = friendsWithMessages.FirstOrDefault();

            if (firstFriend != null)
            {
                receiverId = firstFriend.Id;
            }
        }

        // Передача повідомлень для Partial в Model
        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == userId && m.ReceiverId == receiverId)
                     || (m.SenderId == receiverId && m.ReceiverId == userId))
                .OrderByDescending(m => m.Timestamp) // Спочатку сортуємо за останніми
                    .Take(20) // Обмежуємо вибірку 10 повідомленнями
                        .OrderBy(m => m.Timestamp) // Повертаємо до початкового порядку
                            .ToListAsync();

        // Створюємо копію повідомлень для передачі в View
        var originalMessages = messages.Select(m => new ChatMessage
        {
            Id = m.Id,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId,
            MessageText = m.MessageText,
            Timestamp = m.Timestamp,
            IsRead = m.IsRead,
            MessageImage = m.MessageImage// Зберігаємо початковий стан
        }).ToList();

        // Передача Id відправника і отримувача
        ViewBag.ReceiverId = receiverId;
        ViewBag.SenderId = userId;

        // При переході в чат, помітити всі повідомлення, як отримані
        var unreadMessages = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
        if (unreadMessages.Any())
        {
            unreadMessages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
        }

        return View(originalMessages);
    }
    [HttpPost]
    public async Task<IActionResult> GetUnreadMessagesCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int unreadMessagesCount = await _context.ChatMessages
            .Where(m => m.ReceiverId == int.Parse(userId) && !m.IsRead)
            .CountAsync();

        return Json(new { UnreadCount = unreadMessagesCount });
    }
    [HttpPost]
    public async Task<IActionResult> GetUnreadMessages([FromBody] int receiverId)
    {
        Console.WriteLine($"Received ReceiverId: {receiverId}");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Позначаємо повідомлення від цього ReceiverId як прочитані
        var messagesFromReceiver = await _context.ChatMessages
            .Where(m => m.ReceiverId == int.Parse(userId) && m.SenderId == receiverId && !m.IsRead)
            .ToListAsync();

        if (messagesFromReceiver.Any())
        {
            foreach (var message in messagesFromReceiver)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        // Підраховуємо кількість непрочитаних повідомлень від усіх інших користувачів
        int unreadMessagesCount = await _context.ChatMessages
            .Where(m => m.ReceiverId == int.Parse(userId) && !m.IsRead)
            .CountAsync();

        return Json(new { UnreadCount = unreadMessagesCount });
    }

    [HttpPost]
    public IActionResult GetSenderInfo([FromBody] int senderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        // Шукаємо користувача в контексті за його Id
        var user = _context.Users.FirstOrDefault(u => u.Id == senderId);

        // Перевіряємо, чи знайдено користувача
        if (user == null)
        {
            return NotFound(new { message = "Користувач не знайдений" });
        }

        // Готуємо дані для відповіді
        var senderInfo = new
        {
            fullName = $"{user.FullName}", // ПІБ користувача
            image = Convert.ToBase64String(user.Image) // Перетворення зображення на base64 (якщо воно зберігається як байтовий масив)
        };

        // Повертаємо інформацію про користувача у форматі JSON
        return Ok(senderInfo);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateChatPartial([FromBody] UpdateChatRequest request)
    {
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdstr == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userId = int.Parse(userIdstr);
        int? receiverId = request.ReceiverId;
        int count = request.Count;
        // БЛОК ЯКИЙ ФІЛЬТРУЄ І ГРУПУЄ КОРИСТУВАЧІВ ДЛЯ ОТРИМАННЯ НЕОБХДНИХ ДАННИХ

        // Отримуємо ID друзів з останніми повідомленнями
        var friendIdsWithLastMessage = await _context.ChatMessages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    FriendId = g.Key, // ID друга
                    LastMessageTime = g.OrderByDescending(m => m.Timestamp).Select(m => m.Timestamp).FirstOrDefault(), // для побудови списку
                    LastMessage = g.OrderByDescending(m => m.Timestamp).Select(m => m.MessageText).FirstOrDefault() // для додавання під ПІБ користувача
                })
                    .ToListAsync();

        // Отримуємо друзів за ID
        var friends = await _context.Users
            .Where(u => friendIdsWithLastMessage.Select(f => f.FriendId).Contains(u.Id))
                .Select(user => new
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Image = user.Image
                })
                    .ToListAsync();

        // Отримуємо кількість непрочитаних повідомлень для кожного друга
        var friendsWithUnreadMessages = await _context.ChatMessages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                    .Select(g => new
                    {
                        FriendId = g.Key,
                        UnreadCount = g.Key == receiverId ? 0 : g.Count() // для відображення кількості непрочитаних повідомлень
                    })
                        .ToListAsync();

        // Обєднуєм у кінцевий список для передачі
        var friendsWithMessages = friends
            .Join(friendIdsWithLastMessage,
                 user => user.Id,
                 msg => msg.FriendId,
                 (user, msg) => new
                 {
                     user.Id,
                     user.FullName,
                     user.Image,
                     LastMessageTime = (DateTime?)msg.LastMessageTime,
                     LastMessage = msg.LastMessage,
                     UnreadCount = friendsWithUnreadMessages
                         .FirstOrDefault(f => f.FriendId == user.Id)?.UnreadCount ?? 0 // Кількість непрочитаних повідомлень для усіх у списку
                 })
                    .OrderByDescending(f => f.LastMessageTime)
                        .ToList();

        // Блок для можливості написати користувачу, якого немає у чатах, відкриває чат і поміщає його на верх списка
        var isReceiverInList = friendsWithMessages.Any(f => f.Id == receiverId);
        if (!isReceiverInList)
        {
            var receiver = await _context.Users
                .Where(u => u.Id == receiverId)
                .Select(user => new
                {
                    user.Id,
                    user.FullName,
                    user.Image
                })
                .FirstOrDefaultAsync();

            if (receiver != null)
            {
                var newFriendWithMessage = new
                {
                    receiver.Id,
                    receiver.FullName,
                    receiver.Image,
                    LastMessageTime = (DateTime?)null,
                    LastMessage = "No messages yet",
                    UnreadCount = 0
                };
                friendsWithMessages.Insert(0, newFriendWithMessage);
            }
        }

        // Передача списка користувачів, з яким спілкувався Юзер
        ViewBag.Friends = friendsWithMessages;

        // Блок для завантаження чату першого користувача у чаті
        if (!receiverId.HasValue)
        {
            var firstFriend = friendsWithMessages.FirstOrDefault();

            if (firstFriend != null)
            {
                receiverId = firstFriend.Id;
            }
        }

        // Передача повідомлень для Partial в Model
        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == userId && m.ReceiverId == receiverId)
                     || (m.SenderId == receiverId && m.ReceiverId == userId))
                .OrderByDescending(m => m.Timestamp) // Спочатку сортуємо за останніми
                    .Take(count) // Обмежуємо вибірку 10 повідомленнями
                        .OrderBy(m => m.Timestamp) // Повертаємо до початкового порядку
                            .ToListAsync();

        // Створюємо копію повідомлень для передачі в View
        var originalMessages = messages.Select(m => new ChatMessage
        {
            Id = m.Id,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId,
            MessageText = m.MessageText,
            Timestamp = m.Timestamp,
            IsRead = m.IsRead,
            MessageImage = m.MessageImage// Зберігаємо початковий стан
        }).ToList();

        // Передача Id відправника і отримувача
        ViewBag.ReceiverId = receiverId;
        ViewBag.SenderId = userId;

        // При переході в чат, помітити всі повідомлення, як отримані
        var unreadMessages = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
        if (unreadMessages.Any())
        {
            unreadMessages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
        }
        return PartialView("_ChatWithUser", messages);
    }    
    
    [HttpPost]
    public IActionResult DeleteChat([FromBody] int friendId)
    {
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdstr == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userId = int.Parse(userIdstr);

        // Знаходимо всі повідомлення між поточним користувачем і другом
        var messagesToDelete = _context.ChatMessages
                .Where(m => (m.SenderId == friendId && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == friendId))
                .ToList();

            if (messagesToDelete.Count == 0)
            {
                return NotFound("No chat messages found.");
            }

            // Видаляємо знайдені повідомлення
            _context.ChatMessages.RemoveRange(messagesToDelete);
            _context.SaveChanges();
        return Ok();
    }
}
