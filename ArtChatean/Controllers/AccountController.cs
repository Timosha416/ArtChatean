 using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ArtChatean.Contollers;
public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ArtDbContext _context;
    private readonly IEmailServiceMailKit _emailServiceMailKit;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole<int>> roleManager, ArtDbContext context, IEmailServiceMailKit emailServiceMailKit)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _emailServiceMailKit = emailServiceMailKit;
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;
        return View(new LoginForm());
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;
        return View(new RegisterForm());
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginForm form)
    {
        // Генерація випадкової картинки
        var count = _context.Pictures.Count();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = _context.Pictures.Skip(randomIndex).FirstOrDefault();
        ViewBag.RandomPicture = randomPicture;

        // Знаходження користувача за email
        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null)
        {
            ModelState.AddModelError(nameof(form.Email), "User not found!");
            return View(form);
        }
        // Перевірка, чи підтверджено електронну пошту
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError(nameof(form.Email), "Please confirm your email before logging in!");
            return View(form);
        }

        // Перевірка пароля і логін
        var result = await _signInManager.PasswordSignInAsync(user.UserName, form.Password, false, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(form.Email), "Invalid password!");
            return View(form);
        }

        if (form.RememberMe)
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };
            await _signInManager.SignInAsync(user, authProperties);
        }
        else
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        // Створення ролі Admin, якщо вона не існує
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            await _roleManager.CreateAsync(new IdentityRole<int>() { Name = "Admin" });
        }

        // Додавання користувача до ролі Admin, якщо він ще не є в цій ролі
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        
        // Перенаправлення на головну сторінку
        return Redirect("/Main/Main");
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterForm form)
    {
        var count = _context.Pictures.Count();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = _context.Pictures.Skip(randomIndex).FirstOrDefault();
        ViewBag.RandomPicture = randomPicture;

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var user = await _userManager.FindByEmailAsync(form.Email);

        if (user != null)
        {
            ModelState.AddModelError(nameof(form.Email), "Зазначений користувач уже існує!");
            return View(form);
        }

        user = new User
        {
            Email = form.Email,
            FullName = form.FullName,
            UserName = form.UserName
        };

        var result = await _userManager.CreateAsync(user, form.Password);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(form.Email),
                string.Join(";", result.Errors.ToList().Select(e => e.Description))
                );
            return View(form);
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Створення посилання для підтвердження
        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

        // Відправка листа з підтвердженням через MailKit
        await _emailServiceMailKit.SendEmailAsync(
            user.Email,
            "Email Confirmation",
            $"Please confirm your email by clicking on the link: {confirmationLink}."
        );
        TempData["ConfirmationMessage"] = "To complete your registration, please confirm your email. A confirmation email has been sent to your email address.";
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var count = _context.Pictures.Count();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = _context.Pictures.Skip(randomIndex).FirstOrDefault();
        ViewBag.RandomPicture = randomPicture;

        if (userId == null || token == null)
        {
            return RedirectToAction("Register", "Account");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Не вдалося знайти користувача з ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return View("ConfirmEmailSuccess"); // або перенаправлення на сторінку успішного підтвердження
        }

        return View("Error");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction("Login", "Account");
    }


    // СКИДАННЯ ПАРОЛЮ
    [HttpGet]
    public async Task<IActionResult> ForgotPassword()
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;
        return View();
    }
    
    // Обробка форми скидання пароля
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;

        if (!ModelState.IsValid)
        {
            return View(model);
        }
        Console.WriteLine(model.Email);
        
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))            
        {            
            // Не розкривайте, чи існує користувач
            return RedirectToAction("ForgotPasswordConfirmation", "Account");
        }

        // Генерація токена для скидання пароля
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, protocol: HttpContext.Request.Scheme);

        // Надсилання електронного листа через ваш сервіс
        await _emailServiceMailKit.SendEmailAsync(model.Email, "Password Reset",
            $"You can reset your password by clicking on the link: {callbackUrl}.");
        
        return RedirectToAction("ForgotPasswordConfirmation", "Account");
    }

    // Підтвердження скидання пароля
    [HttpGet]
    public async Task<IActionResult> ForgotPasswordConfirmation()
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;

        return View();
    }

    // Відображення сторінки для скидання пароля
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token = null, string email = null)
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;

        return token == null || email == null ? View("Error") : View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    // Обробка скидання пароля
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(model);
    }

    // Підтвердження скидання пароля
    [HttpGet]
    public async Task<IActionResult> ResetPasswordConfirmation()
    {
        var count = await _context.Pictures.CountAsync();
        var randomIndex = new Random().Next(0, count);
        var randomPicture = await _context.Pictures.Skip(randomIndex).FirstOrDefaultAsync();
        ViewBag.RandomPicture = randomPicture;

        return View();
    }
}
