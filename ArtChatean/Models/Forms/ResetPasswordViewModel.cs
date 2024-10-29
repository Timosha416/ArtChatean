using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "The email field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "The password field is required.")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 3)]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]   
    [Compare("NewPassword", ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "The password reset token is required.")]
    public string Token { get; set; } // Token for confirming password reset
}

