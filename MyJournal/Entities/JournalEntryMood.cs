namespace MyJournal.Entities
{
    public class JournalEntryMood
    {
        public int JournalEntryId { get; set; }
        public JournalEntry JournalEntry { get; set; } = null!;
        
        public int MoodId { get; set; }
        public Mood Mood { get; set; } = null!;
    }
}
