using System.ComponentModel.DataAnnotations;

namespace MyJournal.Entities
{
    public class Mood
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public MoodCategory Category { get; set; }
        
        [StringLength(10)]
        public string? Icon { get; set; }
        
        [StringLength(7)]
        public string? Color { get; set; }
        
        public ICollection<JournalEntryMood> JournalEntries { get; set; } = new List<JournalEntryMood>();
    }

    public enum MoodCategory
    {
        Positive,
        Neutral,
        Negative
    }
}
