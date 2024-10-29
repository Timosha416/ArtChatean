using LiqPay.SDK.Dto;
using LiqPay.SDK;
using Microsoft.AspNetCore.Mvc;
using ArtChatean.Models;
using ArtChatean.Models.Forms;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.Text;
using LiqPay.SDK.Dto.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Org.BouncyCastle.Asn1.X9;

public class PaymentController : Controller
{
    private readonly ArtDbContext _context;
    private readonly string _publicKey = "";
    private readonly string _privateKey = "";

    public PaymentController(ArtDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    public async Task<IActionResult> CheckPayment([FromBody] int orderId)
    {
        //Console.WriteLine("CALLBACK");
        var liqpay = new LiqPayClient(_publicKey, _privateKey);

        // Підготовка параметрів запиту
        var requestParams = new LiqPayRequest
        {
            Version = 3, //Версия Api
            Action = LiqPay.SDK.Dto.Enums.LiqPayRequestAction.Status, //Transaction type           
            OrderId = orderId.ToString(), //Unique order number on your website

        };

        // Викликаємо асинхронний метод для отримання статусу платежу
        var response = await liqpay.RequestAsync("request", requestParams);
        Console.WriteLine(response.Status);
        if (response != null)
        {
            // Повертаємо статус як JSON
            return Json(new { status = response.Status.ToString() });
        }
        else
        {
            // Повертаємо повідомлення про помилку
            return Json(new { status = "No response from LiqPay." });
        }
    }
    // Не актуальний
    [HttpGet]
    public ActionResult Pay(int orderId)
    {        
        var httpContextAccessor = HttpContext.RequestServices.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        
        var order = _context.Orders
        .Include(o => o.PictureOrders) // Включити пов'язані PictureOrders
        .ThenInclude(po => po.Picture) // Включити картинки з PictureOrders (припускаємо, що Picture — це ваша модель для картин)
        .FirstOrDefault(o => o.Id == orderId);

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
            ServerUrl = baseUrl + "/Payment/Redirect" // Callback URL для сповіщень
        };

        //We get a generated form with Data, Signature and a button for payment
        var formCode = liqpay.CNBForm(paymentParams);

        PaymentViewModel pvm = new PaymentViewModel
        {
            PaymentParams = paymentParams,
            FormCode = formCode,
            OrderNumber = order.Number
        };
        return Content($@"
        <html>
        <body onload='document.forms[0].submit()'>
            {formCode}
        </body>
        </html>", "text/html");
    }    
    // Не актуальний
    [HttpPost]
    public ActionResult Redirect()
    {
        Console.WriteLine("CALLBACK");
        // --- Перетворюю відповідь LiqPay в Dictionary<string, string> для зручності:
        var request_dictionary = Request.Form.Keys.ToDictionary(key => key, key => Request.Form[key]);

        // --- Розшифровую параметр data відповіді LiqPay та перетворюю в Dictionary<string, string> для зручності:
        byte[] request_data = Convert.FromBase64String(request_dictionary["data"]);
        string decodedString = Encoding.UTF8.GetString(request_data);
        var request_data_dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedString);

        var liqpay = new LiqPayClient(_publicKey, _privateKey);
        // --- Отримую сигнатуру для перевірки
        var mySignature = liqpay.CreateSignature(request_dictionary["data"]);

        // --- Якщо сигнатура серевера не співпадає з сигнатурою відповіді LiqPay - щось пішло не так
        if (mySignature != request_dictionary["signature"])
            return View("~/Views/Shared/_Error.cshtml");

        // --- Якщо статус відповіді "Тест" або "Успіх" - все добре
        if (request_data_dictionary["status"] == "sandbox" || request_data_dictionary["status"] == "success")
        {
            // Тут можна оновити статус замовлення та зробити всі необхідні речі. Id замовлення можна взяти тут: request_data_dictionary[order_id]
            // ...

            return View("~/Views/Shared/_Success.cshtml");
        }

        return View("~/Views/Shared/_Error.cshtml");
    }
    // Не актуальний
    [HttpPost]
    public async Task<IActionResult> PaymentResult()
    {
        Console.WriteLine("CALLBACK");
        var form = await Request.ReadFormAsync();
        string data = form["data"];
        string signature = form["signature"];

        // Отримайте ключі для валідації
        var liqpay = new LiqPayClient(_publicKey, _privateKey);

        // Перевірте підпис       
        string expectedSignature = liqpay.CreateSignature(data);

        if (expectedSignature == signature)
        {
            // Декодування даних
            var jsonData = Encoding.UTF8.GetString(Convert.FromBase64String(data));
            var paymentResponse = JsonConvert.DeserializeObject<LiqPayResponse>(jsonData);

            if (paymentResponse.Status == LiqPayResponseStatus.Success || paymentResponse.Status == LiqPayResponseStatus.Sandbox)
            {
                Console.WriteLine(paymentResponse.Status);

                // Платіж успішний, повертаємо JSON з відповідним статусом
                return Json(new
                {
                    status = paymentResponse.Status,
                });
            }
            else
            {
                // Платіж неуспішний, повертаємо JSON з помилкою
                return Json(new
                {
                    status = paymentResponse.Status,                    
                });
            }
        }
        else
        {
            // Підпис не вірний, можна згенерувати помилку
            return BadRequest("Невірний підпис.");
        }
    }
}