using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime EventDate { get; set; } // Дата події

        [Required]
        public int AvailableTickets { get; set; } // Кількість доступних квитків

        [Required]
        public int ArtistId { get; set; } // Ідентифікатор художника, до якого належить подія
        [ForeignKey("ArtistId")]
        public Artist Artist { get; set; }

        [Required]
        public string Description { get; set; } // Опис заходу

        public ICollection<EventTimeSlot> TimeSlots { get; set; } // Доступні часові слоти для кожної дати
    }

    public class EventTimeSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Time { get; set; } // Час події (наприклад, 12:00, 14:00 і т.д.)

        [Required]
        public int AvailableTickets { get; set; } // Кількість доступних квитків на конкретний час

        [Required]
        public int EventId { get; set; } // Ідентифікатор події
        [ForeignKey("EventId")]
        public Event Event { get; set; }
    }
    // Модель для тарифних планів
    public class TicketTariff
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Назва тарифу (Full price, Reduced rate, etc.)

        [Required]
        public decimal Price { get; set; } // Ціна за квиток

        [Required]
        public int MaxTicketsPerPerson { get; set; } = 6; // Максимальна кількість квитків на одну людину

        public string Description { get; set; } // Опис тарифу
    }
}