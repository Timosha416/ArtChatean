using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }
        public string? Paypal { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string? Instagram { get; set; }
        public string? Youtube { get; set; }
        public string? Bio { get; set; }
        public List<Picture>? Pictures { get; set; }
        public List<Payment>? Payments { get; set; }
        public List<Address>? Addresses { get; set; }
        public List<Order>? Orders { get; set; }
        public List<Like>? Likes { get; set; }
        public List<Save>? Saves { get; set; }
        public List<Comment>? Comments { get; set; }
        public byte[]? Image { get; set; }
        public List<UserFriend>? UserFriends { get; set; }
        public virtual List<ChatMessage> ChatMessages { get; set; }
        public User()
        {
            Pictures = new List<Picture>();
            Payments = new List<Payment>();
            Addresses = new List<Address>();
            Orders = new List<Order>();
            UserFriends = new List<UserFriend>();
            Likes = new List<Like>();
            Saves = new List<Save>();
            Comments = new List<Comment>();
            ChatMessages = new List<ChatMessage>();
        }        
    }
}
