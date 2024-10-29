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
    public class ArtistController : Controller
    {
        private readonly ArtDbContext _context;

        public ArtistController(ArtDbContext context)
        {
            _context = context;
        }

        public IActionResult Painters()
        {
            var artists = _context.Artists.ToList();
            return View(artists);
        }

        public IActionResult Details(int id)
        {
            var artist = _context.Artists.FirstOrDefault(a => a.Id == id);
            if (artist == null)
            {
                return NotFound();
            }

            return Json(new
            {
                fullName = artist.FullName,
                birthDate = artist.BirthDate,
                birthPlace = artist.BirthPlace,
                deathDate = artist.DeathDate,
                deathPlace = artist.DeathPlace,
                veryBigPhoto = artist.VeryBigPhoto != null ? Convert.ToBase64String(artist.VeryBigPhoto) : null
            });
        }
        public IActionResult Info(int id)
        {
            var artist = _context.Artists.FirstOrDefault(a => a.Id == id);
            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }
    }
}
