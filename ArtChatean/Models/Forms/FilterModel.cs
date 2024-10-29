using System.ComponentModel.DataAnnotations;

namespace ArtChatean.Models.Forms
{
    public class FilterModel
    {
        public List<string>? Style { get; set; } = new List<string>();
        public List<string>? Subject { get; set; } = new List<string>();
        public List<string>? Medium { get; set; } = new List<string>();
        public List<string>? Material { get; set; } = new List<string>();
        public List<decimal?> Price { get; set; } = new List<decimal?>();
        public List<string>? Size { get; set; } = new List<string>();
        public List<string>? Orientation { get; set; } = new List<string>();
        public List<string>? Color { get; set; } = new List<string>();
        public string? UserName { get; set; }
        public int? Page { get; set; } = 1;
    }
}
