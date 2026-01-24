using MyJournal.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyJournal.Models.Input
{
    public class MoodInputModel
    {
        [Required(ErrorMessage = "Mood name is required")]
        [StringLength(50, ErrorMessage = "Mood name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mood category is required")]
        public MoodCategory Category { get; set; }
        
        [StringLength(10, ErrorMessage = "Icon cannot exceed 10 characters")]
        public string? Icon { get; set; }
        
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color")]
        public string? Color { get; set; }
    }
}
