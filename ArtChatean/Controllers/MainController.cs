using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Contollers;

public class MainController : Controller
{
    private readonly ArtDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    public MainController(ArtDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        string base64Image = user.Image != null
        ? Convert.ToBase64String(user.Image)
        : null;

        // Возвращаем данные пользователя, включая изображение в формате Base64
        return Json(new
        {
            Id = user.Id,
            Name = user.FullName,
            AvatarBase64 = base64Image
        });
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        if (model.NewPassword != model.ConfirmPassword)
        {
            return Json(new { success = false, message = "New password and confirmation do not match." });
        }

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        await _signInManager.RefreshSignInAsync(user);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Main()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        ViewBag.UserId = int.Parse(userId);
        ViewBag.UserName = (await _context.Users
            .Where(u => u.Id == int.Parse(userId))
                .FirstOrDefaultAsync())?.FullName;
        var friends = await _context.UserFriends
            .Include(uf => uf.Friend)
                .Where(uf => uf.UserId == int.Parse(userId)).Take(7)
                    .ToListAsync();
        ViewBag.Friends = friends;

        var friendIds = await _context.UserFriends
            .Where(uf => uf.UserId == int.Parse(userId))
                .Select(uf => uf.FriendId)
                    .ToListAsync();

        var friendPictures = await _context.Pictures
            .Where(p => friendIds.Contains(p.User.Id))
                .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                        {
                            p.Id,
                            p.Image,
                            p.Title,
                            p.CreatedAt,
                            p.Caption,
                            LikeCount = p.Likes.Count(),
                            CommentCount = p.Comments.Count(),
                            UserImage = p.User.Image,
                            UserFullName = p.User.FullName,
                            UserId = p.User.Id,
                            IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == int.Parse(userId)),
                            IsSavedByCurrentUser = p.Saves.Any(s => s.UserId == int.Parse(userId)),
                        Comments = p.Comments.Select(c => new
                            {
                            c.Id,
                            c.UserId,
                            c.Text,
                            c.CreatedAt,
                            UserCommentFullName = c.User.FullName,
                            UserCommentId = c.User.Id
                        }).ToList()
                        })
                    .Take(10).ToListAsync();

        return View(friendPictures);
    }
    [HttpPost]
    public async Task<IActionResult> GetPosts([FromBody] int count)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        ViewBag.UserId = int.Parse(userId);
        ViewBag.UserName = (await _context.Users
            .Where(u => u.Id == int.Parse(userId))
                .FirstOrDefaultAsync())?.FullName;
        var friends = await _context.UserFriends
            .Include(uf => uf.Friend)
                .Where(uf => uf.UserId == int.Parse(userId)).Take(7)
                    .ToListAsync();
        ViewBag.Friends = friends;

        var friendIds = await _context.UserFriends
            .Where(uf => uf.UserId == int.Parse(userId))
                .Select(uf => uf.FriendId)
                    .ToListAsync();

        var friendPictures = await _context.Pictures
            .Where(p => friendIds.Contains(p.User.Id))
                .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.Id,
                        p.Image,
                        p.Title,
                        p.CreatedAt,
                        p.Caption,
                        LikeCount = p.Likes.Count(),
                        CommentCount = p.Comments.Count(),
                        UserImage = p.User.Image,
                        UserFullName = p.User.FullName,
                        UserId = p.User.Id,
                        IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == int.Parse(userId)),
                        IsSavedByCurrentUser = p.Saves.Any(s => s.UserId == int.Parse(userId)),
                        Comments = p.Comments.Select(c => new
                        {
                            c.Id,
                            c.UserId,
                            c.Text,
                            c.CreatedAt,
                            UserCommentFullName = c.User.FullName,
                            UserCommentId = c.User.Id
                        }).ToList()
                    })
                    .Skip(count).Take(10).ToListAsync();

        return PartialView("_FriendsPosts", friendPictures);
    }
    [HttpGet]
    public async Task<IActionResult> FriendMain(int id, int pictureId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        ViewBag.UserId = int.Parse(userId);
        ViewBag.UserName = (await _context.Users
            .Where(u => u.Id == int.Parse(userId))
                .FirstOrDefaultAsync())?.FullName;
        var friendPictures = await _context.Pictures
            .Where(p => p.UserId == id && p.IsForGallery == true)
                .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.Id,
                        p.Image,
                        p.Title,
                        p.CreatedAt,
                        p.Caption,
                        LikeCount = p.Likes.Count(),
                        CommentCount = p.Comments.Count(),
                        UserImage = p.User.Image,
                        UserFullName = p.User.FullName,
                        UserId = p.User.Id,
                        IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == int.Parse(userId)),
                        IsSavedByCurrentUser = p.Saves.Any(s => s.UserId == int.Parse(userId)),
                        Comments = p.Comments.Select(c => new
                        {
                            c.Id,
                            c.UserId,
                            c.Text,
                            c.CreatedAt,
                            UserCommentFullName = c.User.FullName,
                            UserCommentId = c.User.Id
                        }).ToList()
                    })
                    .ToListAsync();

        ViewBag.HighlightedPictureId = pictureId;

        return View(friendPictures);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdstr == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var userId = int.Parse(userIdstr);

        var totalLikes = await _context.Pictures
        .Where(p => p.UserId == userId)
            .SelectMany(p => p.Likes)
                .CountAsync();

        var friendCount = await _context.UserFriends
            .CountAsync(uf => uf.UserId == userId);

        var userLikes = await _context.Pictures
            .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LikesCount = g.Sum(p => p.Likes.Count())
                })
                    .OrderByDescending(x => x.LikesCount)
                        .ToListAsync();

        var userRank = userLikes
            .Select((x, index) => new { x.UserId, Rank = index + 1 })
                .FirstOrDefault(x => x.UserId == userId);

        ViewBag.TotalLikes = totalLikes;
        ViewBag.FriendCount = friendCount;
        ViewBag.Rank = userRank != null ? userRank.Rank : 0;

        var user = await _context.Users
            .Include(u => u.Pictures.Where(p => p.IsForGallery))
                .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> FriendProfile(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }


        if (int.Parse(userId) == id)
        {
            return RedirectToAction("Profile", "Main");
        }

        var friend = await _context.Users
            .Include(u => u.Pictures.Where(p => p.IsForGallery))
                .FirstOrDefaultAsync(u => u.Id == id);

        if (friend == null)
        {
            return NotFound("Friend not found");
        }

        var user = await _context.Users
            .Include(u => u.UserFriends)
                .ThenInclude(uf => uf.Friend)
                    .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user == null)
        {
            return NotFound("User not found");
        }

        ViewBag.User = user;

        var totalLikes = await _context.Pictures
            .Where(p => p.UserId == friend.Id)
                .SelectMany(p => p.Likes)
                    .CountAsync();

        var friendCount = await _context.UserFriends
            .CountAsync(uf => uf.UserId == friend.Id);

        var userLikes = await _context.Pictures
            .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LikesCount = g.Sum(p => p.Likes.Count())
                })
                    .OrderByDescending(x => x.LikesCount)
                        .ToListAsync();

        var userRank = userLikes
            .Select((x, index) => new { x.UserId, Rank = index + 1 })
                .FirstOrDefault(x => x.UserId == friend.Id);

        ViewBag.TotalLikes = totalLikes;
        ViewBag.FriendCount = friendCount;
        ViewBag.Rank = userRank != null ? userRank.Rank : 0;

        return View(friend);
    }
    [HttpGet]
    public async Task<IActionResult> Setting()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user == null)
        {
            return NotFound("User not found");
        }

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string query)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var users = await _context.Users
            .Where(u => u.FullName.Contains(query) && u.Id != int.Parse(userId))
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Image 
                })
                    .ToListAsync();

        return Json(users);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(IFormCollection form)
    {
        
        var userIdstr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdstr == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var userId = int.Parse(userIdstr);
        var pictureId = Convert.ToInt32(form["PictureId"]);
        var text = form["Text"].ToString();

        if (string.IsNullOrEmpty(text))
        {
            return BadRequest(new { success = false, message = "Invalid comment data." });
        }

        var newComment = new Comment
        {
            PictureId = pictureId,
            UserId = userId,
            Text = text,
            CreatedAt = DateTime.Now
        };

        await _context.Comments.AddAsync(newComment);
        await _context.SaveChangesAsync();
        
        var user = await _context.Users.FindAsync(newComment.UserId);
        var commentCount = await _context.Comments.CountAsync(c => c.PictureId == pictureId);

        return Ok(new { success = true, username = user.FullName, commentCount, commentId = newComment.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Like([FromBody] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var picture = await _context.Pictures.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);

        if (picture == null)
        {
            return NotFound(new { success = false, message = "Picture not found." });
        }

        var existingLike = picture.Likes.FirstOrDefault(l => l.UserId == int.Parse(userId));
        
        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
        }
        else
        {
            var newLike = new Like { UserId = int.Parse(userId), PictureId = id };
            await _context.Likes.AddAsync(newLike);
        }

        await _context.SaveChangesAsync();
        var likeCount = picture.Likes.Count();
        
        return Ok(new { success = true, likeCount });
    }

    public async Task<IActionResult> Save([FromBody] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var picture = await _context.Pictures.Include(p => p.Saves).FirstOrDefaultAsync(p => p.Id == id);

        if (picture == null)
        {
            return NotFound(new { success = false, message = "Picture not found." });
        }

        var existingSave = picture.Saves.FirstOrDefault(l => l.UserId == int.Parse(userId));

        if (existingSave != null)
        {
            _context.Saves.Remove(existingSave);
        }
        else
        {
            var newSave = new Save { UserId = int.Parse(userId), PictureId = id };
            await _context.Saves.AddAsync(newSave);
        }

        await _context.SaveChangesAsync();      

        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> AddPicture()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users
                                 .Include(u => u.Pictures)
                                    .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user == null)
        {
            return NotFound("User not found");
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFollow([FromBody] int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var currentUser = await _context.Users
            .Include(u => u.UserFriends)
            .FirstOrDefaultAsync(u => u.Id == int.Parse(currentUserId));

        if (currentUser == null)
        {
            Console.WriteLine("Current user not found.");
            return NotFound("User not found");
        }

        var friend = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (friend == null)
        {
            Console.WriteLine($"Friend with ID {id} not found.");
            return NotFound("Friend not found");
        }

        var existingFriendship = currentUser.UserFriends
            .FirstOrDefault(uf => uf.FriendId == id);

        if (existingFriendship != null)
        {
            _context.UserFriends.Remove(existingFriendship);
        }
        else
        {
            await _context.UserFriends.AddAsync(new UserFriend
            {
                UserId = currentUser.Id,
                FriendId = id
            });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Setting(User model, IFormFile? Image)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Перевірка унікальності email
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null && existingUser.Id != int.Parse(userId))
        {
            //ModelState.AddModelError(nameof(model.Email), "Зазначений користувач уже існує!");
            ModelState.AddModelError("Email", "Цей email вже використовується іншим користувачем.");
            return View(model);
        }

        // Оновлення полів профілю
        user.FullName = model.FullName;
        user.Email = model.Email;  // _userManager автоматично нормалізує Email
        user.Paypal = model.Paypal;
        user.PhoneNumber = model.PhoneNumber;
        user.Bio = model.Bio;
        user.Location = model.Location;
        user.Website = model.Website;
        user.Instagram = model.Instagram;
        user.Youtube = model.Youtube;

        if (Image != null && Image.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await Image.CopyToAsync(memoryStream);
                user.Image = memoryStream.ToArray();
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            // Обробка помилок, якщо оновлення не вдалося
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        return Redirect("/Main/Profile");
    }
    [HttpPost]
    public async Task<IActionResult> AddPicture(IFormFile? Image, string Caption)
    {        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            return NotFound("User not found");
        }

        var newPicture = new Picture
        {
            Caption = Caption,
            IsForGallery = true,
            User = user
        };

        if (Image != null && Image.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await Image.CopyToAsync(memoryStream);
                newPicture.Image = memoryStream.ToArray();
            }
        }
        else
        {
            return BadRequest("No image file provided.");
        }
        
        _context.Pictures.Add(newPicture);
        await _context.SaveChangesAsync();

        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> DeletePicture([FromBody] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
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
    [HttpPost]
    public async Task<IActionResult> SalePicture([FromBody] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var picture = await _context.Pictures.FindAsync(id);
        
        if (picture == null)
        {
            return NotFound();
        }

        picture.IsForSale = !picture.IsForSale;

        _context.Pictures.Update(picture);
        await _context.SaveChangesAsync();

        return Ok(); 
    }
    [HttpGet]
    public async Task<IActionResult> LikedProfilePictures()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = int.Parse(currentUserId);

        var likedPictures = await _context.Pictures
            .Where(p => p.Likes.Any(l => l.UserId == userId) && p.UserId != userId && p.IsForGallery == true)
                .Include(p => p.User)
                    .OrderByDescending(p => p.Likes.FirstOrDefault(l => l.UserId == userId).Id)
                        .ToListAsync();

        // Якщо немає лайкнутих картин
        if (!likedPictures.Any())
        {
            return View("LikedProfilePictures", new List<Picture>());
        }

        return View("LikedProfilePictures", likedPictures);
    }

    [HttpGet]
    public async Task<IActionResult> LikedShopPictures()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = int.Parse(currentUserId);

        var likedPictures = await _context.Pictures
            .Where(p => p.Likes.Any(l => l.UserId == userId) && p.IsForSale == true)
                .Include(p => p.User)
                    .OrderByDescending(p => p.Likes.FirstOrDefault(l => l.UserId == userId).Id)
                        .ToListAsync();

        // Якщо немає лайкнутих картин
        if (!likedPictures.Any())
        {
            return View("LikedShopPictures", new List<Picture>());
        }

        return View("LikedShopPictures", likedPictures);
    }

    [HttpGet]
    public async Task<IActionResult> SavedProfilePictures()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = int.Parse(currentUserId);

        var savedPictures = await _context.Pictures
            .Where(p => p.Saves.Any(s => s.UserId == userId) && p.IsForGallery == true)
                .Include(p => p.User)
                    .OrderByDescending(p => p.Saves.FirstOrDefault(l => l.UserId == userId).Id)
                        .ToListAsync();

        // Якщо немає лайкнутих картин
        if (!savedPictures.Any())
        {
            return View("SavedProfilePictures", new List<Picture>());
        }

        return View("SavedProfilePictures", savedPictures);
    }
    [HttpPost]
    public async Task<IActionResult> DeleteComment([FromBody] int id)
    {       
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (currentUserId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
     
        if (comment == null)
        {
            return NotFound("Comment not found");
        }
        
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
       
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> GetUsers([FromBody] int pictureId)
    {
        Console.WriteLine(pictureId);

        var users = await _context.Likes
            .Where(like => like.PictureId == pictureId)
            .Select(like => new {
                Id = like.User.Id, // Додаємо Id користувача
                FullName = like.User.FullName
            })
            .ToListAsync(); // Асинхронний виклик

        return Json(users);
    }
}
