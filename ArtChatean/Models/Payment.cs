using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [CreditCard]
        public string CardNumber { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Range(100, 999, ErrorMessage = "CVV must be a 3-digit number.")]
        public int CVV { get; set; }
        public virtual User User { get; set; }
    }
}
