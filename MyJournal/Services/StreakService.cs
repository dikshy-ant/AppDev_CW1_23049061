using Microsoft.EntityFrameworkCore;
using MyJournal.Data;
using MyJournal.Entities;
using System.Text.Json;

namespace MyJournal.Services
{
    public class StreakService
    {
        private readonly AppDbContext _context;

        public StreakService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StreakTracking> GetStreakDataAsync()
        {
            var streakData = await _context.StreakTracking.FirstOrDefaultAsync();
            
            if (streakData == null)
            {
                // Create default streak data if it doesn't exist
                streakData = new StreakTracking 
                { 
                    Id = 1,
                    LastJournalDate = DateTime.MinValue,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    MissedDaysJson = "[]"
                };
                _context.StreakTracking.Add(streakData);
                await _context.SaveChangesAsync();
            }

            return streakData;
        }

        public async Task UpdateStreakAsync(DateTime journalDate)
        {
            var streakData = await GetStreakDataAsync();
            var today = DateTime.Today;
            
            // If this is the first entry or journal date is before last journal date, recalculate
            if (streakData.LastJournalDate == DateTime.MinValue || journalDate < streakData.LastJournalDate)
            {
                await RecalculateStreakAsync();
                return;
            }

            // Check if this is a consecutive day
            var daysDifference = (today - streakData.LastJournalDate.Date).Days;

            if (daysDifference == 0)
            {
                // Same day - no change to streak
                return;
            }
            else if (daysDifference == 1)
            {
                // Next consecutive day - increment streak
                streakData.CurrentStreak++;
                
                // Update longest streak if needed
                if (streakData.CurrentStreak > streakData.LongestStreak)
                {
                    streakData.LongestStreak = streakData.CurrentStreak;
                }
            }
            else
            {
                // Missed days - reset current streak
                streakData.CurrentStreak = 1;
                
                // Add missed days to tracking
                await AddMissedDaysAsync(streakData, streakData.LastJournalDate, today);
            }

            streakData.LastJournalDate = journalDate;
            await _context.SaveChangesAsync();
        }

        public async Task<StreakStatistics> GetStreakStatisticsAsync()
        {
            var streakData = await GetStreakDataAsync();
            var allEntries = await _context.JournalEntries
                .OrderBy(e => e.Date)
                .ToListAsync();

            var totalJournalDays = allEntries.Count;
            var firstEntryDate = allEntries.FirstOrDefault()?.Date ?? DateTime.Today;
            var daysSinceFirstEntry = (DateTime.Today - firstEntryDate.Date).Days + 1;
            
            var missedDaysList = GetMissedDaysList(streakData.MissedDaysJson);
            var totalMissedDays = missedDaysList.Count;

            return new StreakStatistics
            {
                CurrentStreak = streakData.CurrentStreak,
                LongestStreak = streakData.LongestStreak,
                TotalJournalDays = totalJournalDays,
                TotalPossibleDays = daysSinceFirstEntry,
                TotalMissedDays = totalMissedDays,
                JournalingFrequency = daysSinceFirstEntry > 0 ? (double)totalJournalDays / daysSinceFirstEntry * 100 : 0,
                LastJournalDate = streakData.LastJournalDate,
                MissedDays = missedDaysList.Take(30).ToList() // Last 30 missed days
            };
        }

        public async Task<List<StreakDay>> GetStreakCalendarAsync(int year, int month)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var entriesInMonth = await _context.JournalEntries
                .Where(e => e.Date.Date >= firstDayOfMonth && e.Date.Date <= lastDayOfMonth)
                .ToDictionaryAsync(e => e.Date.Date, e => true);

            var calendarDays = new List<StreakDay>();
            var currentDay = firstDayOfMonth;

            while (currentDay <= lastDayOfMonth)
            {
                calendarDays.Add(new StreakDay
                {
                    Date = currentDay,
                    HasEntry = entriesInMonth.ContainsKey(currentDay.Date),
                    IsToday = currentDay.Date == DateTime.Today
                });
                currentDay = currentDay.AddDays(1);
            }

            return calendarDays;
        }

        private async Task RecalculateStreakAsync()
        {
            var allEntries = await _context.JournalEntries
                .OrderBy(e => e.Date)
                .ToListAsync();

            if (!allEntries.Any())
            {
                var currentStreakRecord = await GetStreakDataAsync();
                currentStreakRecord.CurrentStreak = 0;
                currentStreakRecord.LongestStreak = 0;
                currentStreakRecord.LastJournalDate = DateTime.MinValue;
                currentStreakRecord.MissedDaysJson = "[]";
                await _context.SaveChangesAsync();
                return;
            }

            var currentStreak = 0;
            var longestStreak = 0;
            var lastDate = allEntries.First().Date;
            var missedDays = new List<DateTime>();

            foreach (var entry in allEntries)
            {
                var daysDifference = (entry.Date - lastDate.Date).Days;
                
                if (daysDifference == 0)
                {
                    // Same day - skip
                    continue;
                }
                else if (daysDifference == 1)
                {
                    // Consecutive day
                    currentStreak++;
                }
                else
                {
                    // Missed days
                    for (int i = 1; i < daysDifference; i++)
                    {
                        missedDays.Add(lastDate.AddDays(i));
                    }
                    currentStreak = 1;
                }

                if (currentStreak > longestStreak)
                {
                    longestStreak = currentStreak;
                }

                lastDate = entry.Date;
            }

            var streakRecord = await GetStreakDataAsync();
            streakRecord.CurrentStreak = currentStreak;
            streakRecord.LongestStreak = longestStreak;
            streakRecord.LastJournalDate = lastDate;
            streakRecord.MissedDaysJson = JsonSerializer.Serialize(missedDays);
            
            await _context.SaveChangesAsync();
        }

        private async Task AddMissedDaysAsync(StreakTracking streakData, DateTime fromDate, DateTime toDate)
        {
            var missedDaysList = GetMissedDaysList(streakData.MissedDaysJson);
            
            for (int i = 1; i < (toDate - fromDate).Days; i++)
            {
                var missedDay = fromDate.AddDays(i);
                if (!missedDaysList.Contains(missedDay))
                {
                    missedDaysList.Add(missedDay);
                }
            }

            streakData.MissedDaysJson = JsonSerializer.Serialize(missedDaysList);
            await _context.SaveChangesAsync();
        }

        private List<DateTime> GetMissedDaysList(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<DateTime>>(json) ?? new List<DateTime>();
            }
            catch
            {
                return new List<DateTime>();
            }
        }
    }

    public class StreakStatistics
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalJournalDays { get; set; }
        public int TotalPossibleDays { get; set; }
        public int TotalMissedDays { get; set; }
        public double JournalingFrequency { get; set; }
        public DateTime LastJournalDate { get; set; }
        public List<DateTime> MissedDays { get; set; } = new();
    }

    public class StreakDay
    {
        public DateTime Date { get; set; }
        public bool HasEntry { get; set; }
        public bool IsToday { get; set; }
    }
}
