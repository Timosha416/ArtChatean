using ArtChatean.Models;

namespace ArtChatean.ViewModels
{
    public class PdfTicketsViewModel
    {
        public string ArtistName { get; set; }        // Full name of the artist
        public byte[] ArtistBigPhoto { get; set; }    // Big photo of the artist
        public string EventDescription { get; set; }  // Description of the event
        public int TicketCount { get; set; }          // Number of tickets
        public string TicketDate { get; set; }
        public string TicketTime { get; set; }

        public string ImageUrl { get; set; }
    }
}
