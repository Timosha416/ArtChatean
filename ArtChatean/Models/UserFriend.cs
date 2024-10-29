using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class UserFriend
    {
        public int Id { get; set; } 
        public int UserId { get; set; } 
        public virtual User User { get; set; }
        public int FriendId { get; set; }
        public virtual User Friend { get; set; }
    }
}
