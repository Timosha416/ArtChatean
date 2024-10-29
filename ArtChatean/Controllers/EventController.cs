using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArtChatean.ViewModels;
using Rotativa.AspNetCore;

namespace ArtChatean.Controllers
{
    public class EventController : Controller
    {
        private readonly ArtDbContext _context;
        private readonly IEmailService _emailService;

        public EventController(ArtDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        [Route("Event/Events/{artistId:int}")]
        public IActionResult Events(int artistId)
        {
            Console.WriteLine("Artist ID: " + artistId); // Виводимо ідентифікатор художника для перевірки
            // Запит до бази даних для отримання подій для вказаного художника
            var events = _context.Event
                .Include(e => e.TimeSlots)
                .Where(e => e.ArtistId == artistId && e.EventDate >= DateTime.Today)
                .ToList();
            foreach (var ev in events)
            {
                Console.WriteLine(ev.EventDate);
            }
            // Перевірка наявності подій
            if (!events.Any())
            {
                ViewBag.Message = "No events found."; // Якщо подій немає, передаємо повідомлення
            }

            return View(events); // Передаємо події до представлення
        }

        public IActionResult Details(int eventId)
        {
            var eventDetail = _context.Event
                .Include(e => e.TimeSlots)
                .FirstOrDefault(e => e.Id == eventId);

            if (eventDetail == null)
            {
                return NotFound();
            }

            return View(eventDetail);
        }
        [HttpGet]
        [Route("Event/TicketSelection/{timeSlotId:int}")]
        public IActionResult TicketSelection(int timeSlotId)
        {
            var timeSlot = _context.EventTimeSlot
                .Include(ts => ts.Event)
                .ThenInclude(e => e.Artist)
                .FirstOrDefault(ts => ts.Id == timeSlotId);

            if (timeSlot == null)
            {
                return NotFound();
            }

            var eventDetails = timeSlot.Event;

            // Отримуємо всі доступні тарифи
            var ticketTariffs = _context.TicketTariff.ToList();

            // Передаємо часовий слот і подію через ViewBag
            ViewBag.TimeSlot = timeSlot;
            ViewBag.EventDetails = eventDetails;

            return View(ticketTariffs);
        }

        [HttpPost]
        [Route("Event/Payment/{timeSlotId:int}")]
        public IActionResult Payment(int timeSlotId, Dictionary<int, int> ticketQuantities)
        {
            var timeSlot = _context.EventTimeSlot
                .Include(ts => ts.Event)
                .ThenInclude(e => e.Artist)
                .FirstOrDefault(ts => ts.Id == timeSlotId);

            if (timeSlot == null)
            {
                return NotFound();
            }

            var eventDetails = timeSlot.Event;
            var tariffs = _context.TicketTariff.ToList();

            // Розраховуємо кількість квитків і загальну ціну
            decimal totalPrice = 0;
            int totalTickets = 0;
            var selectedTickets = new List<Tuple<string, int, decimal>>(); // Тариф, кількість, ціна

            foreach (var tariff in tariffs)
            {
                if (ticketQuantities.ContainsKey(tariff.Id))
                {
                    var quantity = ticketQuantities[tariff.Id];
                    if (quantity > 0)
                    {
                        var subTotal = tariff.Price * quantity;
                        totalPrice += subTotal;
                        totalTickets += quantity;
                        selectedTickets.Add(Tuple.Create(tariff.Name, quantity, subTotal));
                    }
                }
            }

            ViewBag.TimeSlot = timeSlot;
            ViewBag.EventDetails = eventDetails;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.TotalTickets = totalTickets;
            ViewBag.SelectedTickets = selectedTickets;

            return View("Payment");
        }

        [HttpPost]
        public async Task<IActionResult> ValidateAndPay(int timeSlotId, int totalTickets, string email, string firstName, string lastName)
        {
            var timeSlot = _context.EventTimeSlot.Include(ts => ts.Event).ThenInclude(e => e.Artist).FirstOrDefault(ts => ts.Id == timeSlotId);

            if (timeSlot == null)
            {
                return NotFound("Time slot not found.");
            }

            if (timeSlot.AvailableTickets < totalTickets)
            {
                return BadRequest("Not enough tickets available.");
            }

            var eventDetails = timeSlot.Event;

            timeSlot.AvailableTickets -= totalTickets;
            _context.SaveChanges();

            // Збереження інформації про покупку квитка
            var ticketPurchase = new TicketPurchase
            {
                BuyerFirstName = firstName,
                BuyerLastName = lastName,
                BuyerEmail = email,
                PurchaseTime = DateTime.Now, // Час покупки
                Quantity = totalTickets,
                ArtistName = eventDetails.Artist.FullName,
                EventDate = eventDetails.EventDate,
                EventTime = timeSlot.Time
            };

            _context.TicketPurchases.Add(ticketPurchase);
            await _context.SaveChangesAsync();

            // Генерація PDF квитків та надсилання листа
            var tickets = new List<byte[]>();
            string imageUrl = $"{Request.Scheme}://{Request.Host}{Url.Content("~/images/screenshot.png")}";

            for (int i = 0; i < totalTickets; i++)
            {
                var viewModel = new PdfTicketsViewModel
                {
                    ArtistName = timeSlot.Event.Artist.FullName,
                    ArtistBigPhoto = eventDetails.Artist.BigPhoto,
                    EventDescription = timeSlot.Event.Description,
                    TicketDate = timeSlot.Time.ToString("dd.MM.yyyy"),
                    TicketTime = timeSlot.Time.ToString(@"hh\:mm"),
                    ImageUrl = imageUrl,
                    TicketCount = 1
                };

                var pdfResult = new ViewAsPdf("PdfTicket", viewModel)
                {
                    FileName = $"Ticket-{i + 1}.pdf"
                };

                var pdfFile = await pdfResult.BuildFile(ControllerContext);
                tickets.Add(pdfFile);
            }

            await _emailService.SendEmailWithAttachmentsAsync(email, "Your Event Tickets", "Thank you for your purchase. Please find your tickets attached.", tickets);

            return RedirectToAction("PdfTickets", new { timeSlotId = timeSlotId, totalTickets = totalTickets });
        }


        [HttpGet]
        [Route("Event/PdfTickets/{timeSlotId:int}")]
        public IActionResult PdfTickets(int timeSlotId, int totalTickets)
        {
            // Fetch the event time slot from the database
            var timeSlot = _context.EventTimeSlot
                .Include(ts => ts.Event)
                .ThenInclude(e => e.Artist) // Завантажити художника разом із подією
                .FirstOrDefault(ts => ts.Id == timeSlotId);


            if (timeSlot == null)
            {
                return NotFound("Time slot not found.");
            }

            var eventDetails = timeSlot.Event;

            if (eventDetails == null)
            {
                return NotFound("Event not found.");
            }
            // Create the ViewModel for the view
            var viewModel = new PdfTicketsViewModel
            {
                ArtistName = eventDetails.Artist.FullName,
                ArtistBigPhoto = eventDetails.Artist.BigPhoto,
                EventDescription = eventDetails.Description,
                TicketDate = timeSlot.Time.ToString("dd.MM.yyyy"),
                TicketTime = timeSlot.Time.ToString(@"hh\:mm"),
                TicketCount = totalTickets
            };
            ViewBag.TimeSlot = timeSlot;
            Console.WriteLine("Ticket Count: " + viewModel.TicketCount);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult DownloadTicket(int timeSlotId)
        {
            Console.WriteLine("Time Slot Id: " + timeSlotId);
            var timeSlot = _context.EventTimeSlot
                .Include(ts => ts.Event)
                .ThenInclude(e => e.Artist)
                .FirstOrDefault(ts => ts.Id == timeSlotId);

            if (timeSlot == null)
            {
                return NotFound("Time slot not found.");
            }

            var eventDetails = timeSlot.Event;

            string imageUrl = $"{Request.Scheme}://{Request.Host}{Url.Content("~/images/screenshot.png")}";

            var viewModel = new PdfTicketsViewModel
            {
                ArtistName = eventDetails.Artist.FullName,
                ArtistBigPhoto = eventDetails.Artist.BigPhoto,
                EventDescription = eventDetails.Description,
                TicketDate = timeSlot.Time.ToString("dd.MM.yyyy"),
                TicketTime = timeSlot.Time.ToString(@"hh\:mm"),
                ImageUrl = imageUrl,
                TicketCount = 1 // Один квиток для кожного завантаження
            };
            ViewBag.TimeSlot = timeSlot;
            // Генеруємо PDF з використанням Rotativa
            return new ViewAsPdf("PdfTicket", viewModel)
            {
                FileName = "Ticket.pdf"
            };
        }


        [HttpPost]
        public IActionResult BookTicket(int timeSlotId)
        {
            var timeSlot = _context.EventTimeSlot.FirstOrDefault(t => t.Id == timeSlotId);

            if (timeSlot == null || timeSlot.AvailableTickets <= 0)
            {
                return BadRequest("Tickets are not available for this time slot.");
            }

            timeSlot.AvailableTickets -= 1;
            _context.SaveChanges();

            return RedirectToAction("Confirmation", new { timeSlotId });
        }

        public IActionResult Confirmation(int timeSlotId)
        {
            var timeSlot = _context.EventTimeSlot
                .Include(t => t.Event)
                .ThenInclude(e => e.Artist)
                .FirstOrDefault(t => t.Id == timeSlotId);

            if (timeSlot == null)
            {
                return NotFound();
            }

            return View(timeSlot);
        }
    }
}