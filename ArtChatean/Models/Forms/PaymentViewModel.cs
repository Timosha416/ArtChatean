using LiqPay.SDK.Dto;
using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms;

public class PaymentViewModel
{
    public LiqPayRequest PaymentParams { get; set; }
    public string FormCode { get; set; }

    public string OrderNumber { get; set; }
}

