using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class Picture
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string? Title { get; set; } = "No title";

        [StringLength(300)]
        public string? Description { get; set; } = "No description";

        [StringLength(300)]
        public string? Caption { get; set; }

        [StringLength(100)]
        public string? Style { get; set; } = "Other";

        [StringLength(100)]
        public string? Subject { get; set; } = "Other";

        [StringLength(100)]
        public string? Medium { get; set; } = "Other";

        [StringLength(100)]
        public string? Material { get; set; } = "Other";

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal? Price { get; set; }
        public string? Size { get; set; } = "100x100 inches";

        [StringLength(20)]
        public string? Orientation { get; set; } = "Other";

        [StringLength(50)]
        public string? Color { get; set; } = "Other";
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public byte[]? Image { get; set; }
        public int? OrderId { get; set; }        
        public virtual List<PictureOrder> PictureOrders { get; set; } = new List<PictureOrder>();
        public virtual List<Like> Likes { get; set; } = new List<Like>();
        public virtual List<Save> Saves { get; set; } = new List<Save>();
        public virtual List<Comment> Comments { get; set; } = new List<Comment>();
        public bool IsForSale { get; set; } = false;
        public bool IsSold { get; set; } = false;
        public bool IsForGallery { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
