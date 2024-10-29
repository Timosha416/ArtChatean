using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms;

public class LoginForm
{
	[EmailAddress]
	[Display(Name = "Email")]
	public string Email { get; set; }
	[Required]
	[Display(Name = "Password")]
	public string Password { get; set; }
    public bool RememberMe { get; set; }
}

