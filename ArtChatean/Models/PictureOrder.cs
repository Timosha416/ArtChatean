using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class PictureOrder
    {
        [Key]
        public int Id { get; set; }

        [Required] 
        public int PictureId { get; set; }

        [ForeignKey(nameof(PictureId))]
        public Picture Picture { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }
    }
}
