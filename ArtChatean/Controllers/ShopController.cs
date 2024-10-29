using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Collections.Specialized.BitVector32;
using System.Linq;

namespace ArtChatean.Contollers;

public class ShopController : Controller
{
    private readonly ArtDbContext _context;
    private const int PageSize = 15;
    public ShopController(ArtDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Main(string section)
    {
        ViewBag.SelectedSection = section ?? "PictureGallery";
        
        section = ViewBag.SelectedSection;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.currentUserId = int.Parse(currentUserId);

        var pictures = GetFilteredPictures(null, int.Parse(currentUserId), section).Include(p => p.PictureOrders).ThenInclude(po => po.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Addresses).Include(p => p.User).ToList();

        ViewBag.CurrentPage = 1;
        currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(pictures);
    }
    [HttpPost]
    public async Task<IActionResult> Filter([FromBody] FilterModel? filters)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.currentUserId = int.Parse(currentUserId);

        var filteredPictures = GetFilteredPictures(filters, int.Parse(currentUserId), "PictureGallery").ToList();

        currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.CurrentPage = filters.Page;

        return PartialView("_PictureGallery", filteredPictures);
    }
    private IQueryable<Picture> GetFilteredPictures(FilterModel? filters, int currentUserId, string section)
    {
        var query = _context.Pictures.Include(p => p.Likes)
            .Include(p => p.PictureOrders).ThenInclude(o => o.Order)
                .Include(p => p.User)
                    .Where(p => p.IsForSale && !p.IsSold && p.UserId != currentUserId)
                        .AsQueryable();

        ViewBag.ValueForFilters = query.ToList();

        if (section == "PictureGallery")
        {
            if (filters != null)
            {
                if (filters.Style != null && filters.Style.Any())
                {
                    query = query.Where(p => filters.Style.Contains(p.Style));
                }
                if (filters.Subject != null && filters.Subject.Any())
                {
                    query = query.Where(p => filters.Subject.Contains(p.Subject));
                }
                if (filters.Medium != null && filters.Medium.Any())
                {
                    query = query.Where(p => filters.Medium.Contains(p.Medium));
                }
                if (filters.Material != null && filters.Material.Any())
                {
                    query = query.Where(p => filters.Material.Contains(p.Material));
                }
                if (filters.Price != null && filters.Price.Count == 2)
                {
                    decimal priceFrom = filters.Price[0] ?? 0; 
                    decimal priceTo = filters.Price[1] ?? decimal.MaxValue; 

                    query = query.Where(p => p.Price.HasValue && p.Price.Value >= priceFrom && p.Price.Value <= priceTo);
                }
                if (filters.Size != null && filters.Size.Any())
                {
                    query = query.Where(p => filters.Size.Contains(p.Size));
                }
                if (filters.Orientation != null && filters.Orientation.Any())
                {
                    query = query.Where(p => filters.Orientation.Contains(p.Orientation));
                }
                if (filters.Color != null && filters.Color.Any())
                {
                    query = query.Where(p => filters.Color.Contains(p.Color));
                }
                if (!string.IsNullOrEmpty(filters.UserName))
                {
                    query = query.Where(p => p.User.FullName != null && p.User.FullName.Contains(filters.UserName));
                }
            }

            ViewBag.TotalPages = (int)Math.Ceiling(query.Count() / (double)PageSize);
            
            if (filters == null)
            {                
                return query.Skip(0).Take(PageSize);
            }
            
            int page = (filters.Page ?? 1) - 1;

            return query.Skip(page * PageSize).Take(PageSize);
        }
        else
        {
            query = _context.Pictures.Where(p => p.UserId == currentUserId && p.IsForSale).AsQueryable();
            return query;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Picture(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.currentUserId = int.Parse(currentUserId);

        var picture = await _context.Pictures
            .Include(p => p.User)
                .Include(po => po.PictureOrders)
                    .ThenInclude(o => o.Order)
                        .Include(p => p.User.Pictures)
                            .FirstOrDefaultAsync(p => p.Id == id);

        if (picture == null)
        {
            return NotFound();
        }

        return View(picture);
    }    

    [HttpPost]
    public async Task<IActionResult> Like([FromBody] int id)
    {
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdstr == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var userId = int.Parse(userIdstr);

        var picture = await _context.Pictures
            .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);

        if (picture == null)
        {
            return NotFound(new { success = false, message = "Picture not found." });
        }

        var existingLike = picture.Likes.FirstOrDefault(l => l.UserId == userId);
        
        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
        }
        else
        {
            var newLike = new Like { UserId = userId, PictureId = id };
            _context.Likes.Add(newLike);
        }

        await _context.SaveChangesAsync();

        var likeCount = picture.Likes.Count;
        
        return Ok(new { success = true, likeCount });
    }

    [HttpPost]
    public async Task<IActionResult> AddPicture(Picture model, IFormFile? Image)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var user = await _context.Users.FindAsync(model.UserId);
        
        if (user == null)
        {
            return NotFound("User not found");
        }
        
        var newPicture = new Picture
        {
            Title = model.Title,
            Description = model.Description,
            Caption = model.Caption,
            Style = model.Style,
            Subject = model.Subject,
            Medium = model.Medium,
            Material = model.Material,
            Price = model.Price,
            Size = model.Size,
            Orientation = model.Orientation,
            Color = model.Color,
            UserId = model.UserId,
            User = user,
            IsForSale = true,
            IsForGallery = model.IsForGallery
        };
       
        if (Image != null && Image.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await Image.CopyToAsync(memoryStream);
                newPicture.Image = memoryStream.ToArray();
            }
        }
        
        _context.Pictures.Add(newPicture);
        await _context.SaveChangesAsync();

        return Redirect($"/Shop/Main?section=AddEditPicture");
    }

    [HttpPost]
    public async Task<IActionResult> EditPicture(Picture model, IFormFile? Image)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var existingPicture = await _context.Pictures.FindAsync(model.Id);
       
        if (existingPicture == null)
        {
            return NotFound("Picture not found");
        }

        existingPicture.Title = model.Title;
        existingPicture.Description = model.Description;
        existingPicture.Caption = model.Caption;
        existingPicture.Style = model.Style;
        existingPicture.Subject = model.Subject;
        existingPicture.Medium = model.Medium;
        existingPicture.Material = model.Material;
        existingPicture.Price = model.Price;
        existingPicture.Size = model.Size;
        existingPicture.Orientation = model.Orientation;
        existingPicture.Color = model.Color;
        existingPicture.IsForSale = true;
        existingPicture.IsForGallery = model.IsForGallery;
        
        if (Image != null && Image.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await Image.CopyToAsync(memoryStream);
                existingPicture.Image = memoryStream.ToArray();
            }
        }

        _context.Pictures.Update(existingPicture);
        await _context.SaveChangesAsync();

        return Redirect($"/Shop/Main?section=AddEditPicture");
    }

    [HttpPost]
    public async Task<IActionResult> DeletePicture([FromBody] int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var picture = await _context.Pictures.FindAsync(id);
       
        if (picture == null)
        {
            return NotFound();
        }

        _context.Pictures.Remove(picture);
        await _context.SaveChangesAsync();
       
        return Ok();
    }
}
