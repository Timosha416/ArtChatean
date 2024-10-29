using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ArtDbContext _context;

        public GalleryController(ArtDbContext context)
        {
            _context = context;
        }

        // Показати картини для вибраної ери
        public IActionResult Pictures(int? eraId)
        {
            // Якщо ера не вибрана, беремо першу еру за замовчуванням
            var selectedEra = eraId.HasValue
                ? _context.Eras.Include(e => e.Paintings).FirstOrDefault(e => e.Id == eraId)
                : _context.Eras.Include(e => e.Paintings).OrderBy(e => e.StartYear).FirstOrDefault();

            if (selectedEra == null)
            {
                return NotFound("Era not found");
            }

            ViewBag.SelectedEra = selectedEra;
            ViewBag.Eras = _context.Eras.ToList();
            return View(selectedEra.Paintings.OrderBy(p => p.YearCreated).ToList());
        }

        // Перегляд ери для вибору
        public IActionResult SelectEra()
        {
            var eras = _context.Eras.ToList();
            return View(eras);
        }
    }
}
