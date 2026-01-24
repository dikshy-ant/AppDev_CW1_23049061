using System.ComponentModel.DataAnnotations;

namespace MyJournal.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        public bool IsPreDefined { get; set; } = false;
        
        [StringLength(7)]
        public string? Color { get; set; }
        
        public ICollection<JournalEntryTag> JournalEntries { get; set; } = new List<JournalEntryTag>();
    }
}
