using MyJournal.Entities;

namespace MyJournal.Models.Display
{
    public class JournalEntryDisplayModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ContentPreview { get; set; } = string.Empty;
        public string PlainTextPreview { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Mood information
        public MoodDisplayModel? PrimaryMood { get; set; }
        public List<MoodDisplayModel> SecondaryMoods { get; set; } = new List<MoodDisplayModel>();
        
        // Category and tags
        public CategoryDisplayModel? Category { get; set; }
        public List<TagDisplayModel> Tags { get; set; } = new List<TagDisplayModel>();
        
        // UI helper properties
        public string FormattedDate { get; set; } = string.Empty;
        public string WordCount { get; set; } = string.Empty;
        public bool IsToday => Date.Date == DateTime.Today;
        public bool WasEdited => UpdatedAt > CreatedAt.AddMinutes(1);
    }
    
    public class MoodDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public MoodCategory Category { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string CategoryDisplay => Category.ToString();
    }
    
    public class CategoryDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
    }
    
    public class TagDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPreDefined { get; set; }
        public string? Color { get; set; }
    }
}
