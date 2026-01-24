namespace MyJournal.Entities
{
    public class JournalEntryTag
    {
        public int JournalEntryId { get; set; }
        public JournalEntry JournalEntry { get; set; } = null!;
        
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
