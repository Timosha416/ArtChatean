using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PictureId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;      
        public virtual Picture Picture { get; set; }
        public virtual User User { get; set; }
    }
}
