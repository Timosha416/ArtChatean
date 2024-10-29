using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class Save
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int PictureId { get; set; }
        public virtual Picture Picture { get; set; }
    }
}
