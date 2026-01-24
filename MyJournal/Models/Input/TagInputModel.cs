using System.ComponentModel.DataAnnotations;

namespace MyJournal.Models.Input
{
    public class TagInputModel
    {
        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;
        
        public bool IsPreDefined { get; set; } = false;
        
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color")]
        public string? Color { get; set; }
    }
}
