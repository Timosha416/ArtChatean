using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    [Table("Painting")]
    public class Painting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } // Назва картини

        [Required]
        [StringLength(100)]
        public string Author { get; set; } // Автор картини

        [Required]
        public int YearCreated { get; set; } // Рік створення картини

        [Required]
        public int EraId { get; set; } // Ідентифікатор ери, до якої належить картина

        [ForeignKey("EraId")]
        public virtual Era Era { get; set; }

        public byte[] Image { get; set; } // Зображення картини
    }
}
