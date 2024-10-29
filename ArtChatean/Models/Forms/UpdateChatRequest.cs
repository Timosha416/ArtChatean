using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms;

public class UpdateChatRequest
{
    public int ReceiverId { get; set; }
    public int Count { get; set; }
}

