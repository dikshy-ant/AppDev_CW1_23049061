using MyJournal.Entities;

namespace MyJournal.Models.Display
{
    public class AnalyticsDisplayModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalEntries { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int MissedDays { get; set; }
        
        // Summary stats
        public int TotalWords { get; set; }
        public double AverageWordsPerEntry { get; set; }
        public int EntriesThisWeek { get; set; }
        public int EntriesThisMonth { get; set; }
        public string MostProductiveDayOfWeek { get; set; } = string.Empty;
        public int LongestEntryWords { get; set; }
        public int ShortestEntryWords { get; set; }
        public string MostFrequentMood { get; set; } = string.Empty;
        public int UniqueTagsCount { get; set; }
        
        // Mood analytics
        public List<MoodFrequencyDisplayModel> MoodDistribution { get; set; } = new();
        public MoodDisplayModel? MostFrequentMoodModel { get; set; }
        
        // Tag analytics
        public List<TagFrequencyDisplayModel> TopTags { get; set; } = new();
        public List<CategoryBreakdownDisplayModel> CategoryBreakdown { get; set; } = new();
        
        // Writing analytics
        public List<WordCountTrendDisplayModel> WordCountTrend { get; set; } = new();
    }
    
    public class MoodFrequencyDisplayModel
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public MoodCategory Category { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
    
    public class TagFrequencyDisplayModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
    
    public class CategoryBreakdownDisplayModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public double Percentage { get; set; }
    }
    
    public class WordCountTrendDisplayModel
    {
        public DateTime Date { get; set; }
        public int WordCount { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
        public MoodDisplayModel? PrimaryMood { get; set; }
    }
}
