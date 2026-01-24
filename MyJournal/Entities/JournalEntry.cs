using System.ComponentModel.DataAnnotations;

namespace MyJournal.Entities
{
    public class JournalEntry
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int? PrimaryMoodId { get; set; }
        public int? CategoryId { get; set; }
        
        // Navigation properties
        public Mood? PrimaryMood { get; set; }
        public Category? Category { get; set; }
        
        // Collections
        public ICollection<JournalEntryMood> SecondaryMoods { get; set; } = new List<JournalEntryMood>();
        public ICollection<JournalEntryTag> Tags { get; set; } = new List<JournalEntryTag>();
    }
}
