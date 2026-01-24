using System.ComponentModel.DataAnnotations;

namespace MyJournal.Models.Input
{
    public class JournalEntryInputModel
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "Content is required")]
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        [MaxLength(10000, ErrorMessage = "Content cannot exceed 10,000 characters")]
        public string Content { get; set; } = string.Empty;
        
        // Additional fields for the form
        public string Title { get; set; } = string.Empty;
        public string Feelings { get; set; } = string.Empty;
        public string HappenedToday { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Primary mood is required")]
        public int PrimaryMoodId { get; set; }
        
        public int? CategoryId { get; set; }
        
        // Secondary moods (max 2)
        [Range(0, 2, ErrorMessage = "You can select up to 2 secondary moods")]
        public List<int> SecondaryMoodIds { get; set; } = new List<int>();
        
        // Tags
        public List<int> TagIds { get; set; } = new List<int>();
        
        // For creating new tags on the fly
        public List<string> NewTagNames { get; set; } = new List<string>();
    }
}
