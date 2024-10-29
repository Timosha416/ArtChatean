using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [ForeignKey("Seller")]
        public int SellerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Number { get; set; }
        public bool IsCompleted { get; set; } = false;

        [Required]
        public DateTime OrderDate { get; set; }
        public virtual User Customer { get; set; }
        public virtual User Seller { get; set; }
        public virtual ICollection<PictureOrder> PictureOrders { get; set; } = new List<PictureOrder>();
    }
}

