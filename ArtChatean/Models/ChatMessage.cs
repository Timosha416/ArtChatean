using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class ChatMessage
    {
        public int Id { get; set; } 

        public int SenderId { get; set; } 
        public virtual User Sender { get; set; }

        public int ReceiverId { get; set; } 
        public virtual User Receiver { get; set; }

        public string? MessageText { get; set; }
        public byte[]? MessageImage { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } 
    }
}
