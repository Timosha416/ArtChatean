using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;
using LiqPay.SDK.Dto;
using LiqPay.SDK;
using MailKit.Search;

namespace ArtChatean.Contollers;

public class OrderController : Controller
{
    private readonly ArtDbContext _context;
    private readonly IEmailServiceMailKit _emailServiceMailKit;
    private readonly string _publicKey = "";
    private readonly string _privateKey = "";
    public OrderController(ArtDbContext context, IEmailServiceMailKit emailServiceMailKit)
    {
        _context = context;
        _emailServiceMailKit = emailServiceMailKit;
    }

    [HttpGet]
    public async Task<IActionResult> Main(string section)
    {
        ViewBag.SelectedSection = section ?? "ActiveOrders";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userIdInt = int.Parse(userId);
        var user = await _context.Users
            .Include(u => u.Addresses)
                .Include(u => u.Payments)
                    .Include(u => u.Orders.Where(o => o.CustomerId == userIdInt))
                        .ThenInclude(o => o.PictureOrders)
                            .ThenInclude(po => po.Picture).ThenInclude(u => u.User)
                                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user.Orders == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userIdInt = int.Parse(userId);

        var user = await _context.Users
            .Include(u => u.Addresses)
                .Include(u => u.Payments)
                    .Include(u => u.Orders
                    .Where(o => o.CustomerId == userIdInt && o.IsCompleted == false))
                        .ThenInclude(o => o.PictureOrders)
                            .ThenInclude(po => po.Picture)
                                .ThenInclude(p => p.User)
                                    .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user.Orders == null)
        {
            return NotFound();
        }

        var httpContextAccessor = HttpContext.RequestServices.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

        var order = user.Orders.FirstOrDefault();

        var liqpay = new LiqPayClient(_publicKey, _privateKey);

        string baseUrl = HttpContext.Request.Scheme + "://" + httpContextAccessor.HttpContext.Request.Host.Value;

        //Payment parameters
        var paymentParams = new LiqPayRequest
        {
            Version = 3, //Версия Api
            Action = LiqPay.SDK.Dto.Enums.LiqPayRequestAction.Pay, //Transaction type
            Amount = (double)order.PictureOrders.Sum(po => po.Picture.Price), //Payment amount
            Sandbox = "1",
            Currency = "USD", //Payment currency
            Description = "Оплата тестового продукта - #" + order.Number, //Payment Description 
            OrderId = order.Id.ToString(), //Unique order number on your website
            ResultUrl = baseUrl + "/Order?section=DoneOrders", //Link to successful payment page            
        };

        //We get a generated form with Data, Signature and a button for payment
        var formCode = liqpay.CNBForm(paymentParams);
        
        ViewBag.FormCode = formCode;
        Console.WriteLine(formCode);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Finish()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userIdInt = int.Parse(userId);

        var user = await _context.Users
            .Include(u => u.Addresses)
                .Include(u => u.Payments)
                    .Include(u => u.Orders
                        .Where(o => o.CustomerId == userIdInt))
                            .ThenInclude(o => o.PictureOrders)
                                .ThenInclude(po => po.Picture)
                                    .ThenInclude(p => p.User)
                                        .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user.Orders == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> DeletePictureOrder([FromBody] int pictureOrderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        try
        {
            var pictureOrder = await _context.PictureOrders
                .SingleOrDefaultAsync(po => po.Id == pictureOrderId);            
            if (pictureOrder != null)
            {

                var order = await _context.Orders.FindAsync(pictureOrder.OrderId);
                _context.PictureOrders.Remove(pictureOrder);
                await _context.SaveChangesAsync();

                var remainingPictures = _context.PictureOrders.Where(po => po.OrderId == order.Id).Any();

                if (!remainingPictures)
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            return NotFound("Picture order not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while deleting the picture order.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CompleteOrder([FromBody] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var order = await _context.Orders
            .Include(o => o.PictureOrders)
                .ThenInclude(po => po.Picture)
                    .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        bool hasConflict = false;

        foreach (var pictureOrder in order.PictureOrders)
        {
            if (pictureOrder.Picture.IsSold)
            {
                hasConflict = true;
            }
        }

        if (hasConflict)
        {
            return Conflict("Some pictures were already sold.");
        }

        foreach (var pictureOrder in order.PictureOrders)
        {
            pictureOrder.Picture.IsSold = true;
        }

        order.IsCompleted = true;

        await _context.SaveChangesAsync();

        foreach (var pictureOrder in order.PictureOrders)
        {
            var sellerEmail = pictureOrder.Picture.User.Email;
            var sellerName = pictureOrder.Picture.User.FullName;
            var orderNumder = pictureOrder.Order.Number;

            if (!string.IsNullOrEmpty(sellerEmail))
            {
                var emailSubject = "Artwork Sold";
                var emailBody = $"Dear {sellerName}, your artwork {pictureOrder.Picture.Title} has been sold. Order № {orderNumder}. You can view it in your Sold Pictures.";

                // Використання MailKit для відправки
                await _emailServiceMailKit.SendEmailAsync(
                    sellerEmail,
                    emailSubject,
                    emailBody
                );
            }
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> AddToOrder([FromBody] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var user = await _context.Users
                            .Include(u => u.Orders)
                                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

        if (user == null)
        {
            return NotFound();
        }

        var picture = await _context.Pictures.FindAsync(id);
        if (picture == null)
        {
            return NotFound();
        }

        var order = user.Orders.FirstOrDefault(o => !o.IsCompleted);
        if (order == null)
        {
            order = new Order
            {
                CustomerId = user.Id,
                OrderDate = DateTime.Now,
                IsCompleted = false,
                Number = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                SellerId = picture.UserId
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        var pictureOrder = new PictureOrder
        {
            OrderId = order.Id,
            PictureId = picture.Id
        };

        _context.PictureOrders.Add(pictureOrder);
        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveAddress(Address address)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userIdInt = int.Parse(userId);

        var user = await _context.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.Id == userIdInt);

        if (user != null)
        {
            var existingAddress = user.Addresses.FirstOrDefault(a => a.Id == address.Id);
            if (existingAddress != null)
            {
                existingAddress.Country = address.Country;
                existingAddress.PostalCode = address.PostalCode;
                existingAddress.City = address.City;
                existingAddress.Street = address.Street;
            }
            else
            {
                var newAddress = new Address
                {
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    City = address.City,
                    Street = address.Street,
                    User = user
                };
                user.Addresses.Add(newAddress);
            }
            await _context.SaveChangesAsync();
        }
        else
        {
            TempData["Message"] = "User not found.";
            return Redirect("/Order/Main?section=PaymentMethods");
        }

        TempData["Message"] = "Address saved successfully!";

        return Redirect("/Order/Main?section=Addresses");
    }

    [HttpPost]
    public async Task<IActionResult> SavePayment(Payment payment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userIdInt = int.Parse(userId);

        var user = await _context.Users.Include(u => u.Payments)
                                       .FirstOrDefaultAsync(u => u.Id == userIdInt);

        if (user != null)
        {
            var existingPayment = user.Payments.FirstOrDefault(p => p.Id == payment.Id);
            if (existingPayment != null)
            {
                existingPayment.CardNumber = payment.CardNumber;
                existingPayment.CardHolderName = payment.CardHolderName;
                existingPayment.ExpiryDate = payment.ExpiryDate;
                existingPayment.CVV = payment.CVV;
            }
            else
            {
                var newPayment = new Payment
                {
                    CardNumber = payment.CardNumber,
                    CardHolderName = payment.CardHolderName,
                    ExpiryDate = payment.ExpiryDate,
                    CVV = payment.CVV
                };
                user.Payments.Add(newPayment);
            }
            await _context.SaveChangesAsync();
        }
        else
        {
            TempData["Message"] = "User not found.";
            return Redirect("/Order/Main?section=PaymentMethods");
        }
        TempData["Message"] = "Payment method saved successfully!";
        return Redirect("/Order/Main?section=PaymentMethods");
    }
}
