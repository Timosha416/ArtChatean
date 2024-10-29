using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    [Table("TicketPurchase")]
    public class TicketPurchase
    {
        [Key]
        public int Id { get; set; }
        public string BuyerFirstName { get; set; }
        public string BuyerLastName { get; set; }
        public string BuyerEmail { get; set; }
        public DateTime PurchaseTime { get; set; }
        public int Quantity { get; set; }
        public string ArtistName { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EventTime { get; set; }
    }

}
