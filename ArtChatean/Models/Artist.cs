using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    [Table("Artists")]
    public class Artist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } // Ім'я художника

        [Required]
        public DateTime BirthDate { get; set; } // Дата народження

        public DateTime? DeathDate { get; set; } // Дата смерті (якщо помер)

        [Required]
        [StringLength(100)]
        public string BirthPlace { get; set; } // Місце народження

        [StringLength(100)]
        public string DeathPlace { get; set; } // Місце смерті

        [StringLength(1000)]
        public string Bio { get; set; } // Біографія

        [StringLength(200)]
        public string Quote { get; set; } // Цитата художника

        public byte[]? Photo { get; set; } // Фото художника

        public byte[]? BigPhoto { get; set; } // Велике фото художника

        public byte[]? VeryBigPhoto { get; set; } // Дуже велике фото художника

        public byte[]? Picture1 { get; set; } // Картина художника

        public byte[]? Picture2 { get; set; } // Картина художника

        public byte[]? Picture3 { get; set; } // Картина художника

        public byte[]? Picture4 { get; set; } // Картина художника

        public byte[]? Picture5 { get; set; } // Картина художника

        public byte[]? Picture6 { get; set; } // Картина художника

        public byte[]? HeaderPicture { get; set; } // Верхня картина художника

        [StringLength(255)]
        public string Video { get; set; } // Відео про художника
        // Колекція картин
        public virtual ICollection<Picture> Pictures { get; set; }

        public Artist()
        {
            Pictures = new List<Picture>();
        }
    }
}
