﻿using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms;

public class ChangePasswordViewModel
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}
