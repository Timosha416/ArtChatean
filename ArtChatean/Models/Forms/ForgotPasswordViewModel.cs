using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Поле електронної пошти обов'язкове.")]
    [EmailAddress(ErrorMessage = "Некоректний формат електронної пошти.")]
    public string Email { get; set; }
}

