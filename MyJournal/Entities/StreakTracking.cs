using System.ComponentModel.DataAnnotations;

namespace MyJournal.Entities
{
    public class StreakTracking
    {
        public int Id { get; set; }
        
        public DateTime LastJournalDate { get; set; }
        
        public int CurrentStreak { get; set; } = 0;
        
        public int LongestStreak { get; set; } = 0;
        
        public string MissedDaysJson { get; set; } = "[]"; // Store as JSON string
    }
}
