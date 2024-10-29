using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms;

public class RegisterForm
{
	[Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }
    [Display(Name = "E-mail")]
    public string Email { get; set; }
    [Display(Name = "Username")]
    public string UserName { get; set; }
    [Display(Name = "Password")]
    public string Password { get; set; }
    [Display(Name = "ConfirmPassword")]
    public string ConfirmPassword { get; set; }
}
