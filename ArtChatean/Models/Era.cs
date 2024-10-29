using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    [Table("Era")]
    public class Era
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Назва ери

        [Required]
        public int StartYear { get; set; } // Рік початку ери

        [Required]
        public int EndYear { get; set; } // Рік закінчення ери

        // Зв'язок один до багатьох з картинами
        public virtual ICollection<Painting> Paintings { get; set; }

        public Era()
        {
            Paintings = new List<Painting>();
        }
    }
}
