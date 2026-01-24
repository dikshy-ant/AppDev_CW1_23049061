using Microsoft.EntityFrameworkCore;
using MyJournal.Common;
using MyJournal.Data;
using MyJournal.Entities;
using MyJournal.Models.Input;
using MyJournal.Models.Display;
using System.Linq.Expressions;

namespace MyJournal.Services
{
    public class JournalService
    {
        private readonly AppDbContext _context;
        private readonly StreakService _streakService;

        public JournalService(AppDbContext context, StreakService streakService)
        {
            _context = context;
            _streakService = streakService;
        }

        public async Task<ServiceResult<JournalEntryDisplayModel>> CreateEntryAsync(JournalEntryInputModel inputModel)
        {
            try
            {
                // Check if entry already exists for this date
                var existingEntry = await _context.JournalEntries
                    .FirstOrDefaultAsync(je => je.Date.Date == inputModel.Date.Date);

                if (existingEntry != null)
                {
                    return ServiceResult<JournalEntryDisplayModel>.Fail("An entry already exists for this date. Only one entry per day is allowed.");
                }

                // Create new journal entry
                var journalEntry = new JournalEntry
                {
                    Date = inputModel.Date.Date,
                    Content = inputModel.Content,
                    PrimaryMoodId = inputModel.PrimaryMoodId,
                    CategoryId = inputModel.CategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add secondary moods
                foreach (var moodId in inputModel.SecondaryMoodIds.Take(2)) // Max 2 secondary moods
                {
                    journalEntry.SecondaryMoods.Add(new JournalEntryMood
                    {
                        MoodId = moodId
                    });
                }

                // Add existing tags
                foreach (var tagId in inputModel.TagIds)
                {
                    journalEntry.Tags.Add(new JournalEntryTag
                    {
                        TagId = tagId
                    });
                }

                // Add new tags
                foreach (var tagName in inputModel.NewTagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var existingTag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

                    if (existingTag == null)
                    {
                        var newTag = new Tag
                        {
                            Name = tagName.Trim(),
                            IsPreDefined = false,
                            Color = "#6C5CE7" // Default color for custom tags
                        };
                        _context.Tags.Add(newTag);
                        await _context.SaveChangesAsync();

                        journalEntry.Tags.Add(new JournalEntryTag
                        {
                            TagId = newTag.Id
                        });
                    }
                    else
                    {
                        journalEntry.Tags.Add(new JournalEntryTag
                        {
                            TagId = existingTag.Id
                        });
                    }
                }

                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync();

                // Update streak tracking
                await _streakService.UpdateStreakAsync(journalEntry.Date);

                // Load related data for display
                var createdEntry = await _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .FirstAsync(je => je.Id == journalEntry.Id);

                var displayModel = MapToDisplayModel(createdEntry);

                return ServiceResult<JournalEntryDisplayModel>.Ok(displayModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<JournalEntryDisplayModel>.Fail($"Error creating journal entry: {ex.Message}");
            }
        }

        public async Task<ServiceResult<JournalEntryDisplayModel>> UpdateEntryAsync(int id, JournalEntryInputModel inputModel)
        {
            try
            {
                var existingEntry = await _context.JournalEntries
                    .Include(je => je.SecondaryMoods)
                    .Include(je => je.Tags)
                    .FirstOrDefaultAsync(je => je.Id == id);

                if (existingEntry == null)
                {
                    return ServiceResult<JournalEntryDisplayModel>.Fail("Entry not found.");
                }

                // Update entry properties
                existingEntry.Content = inputModel.Content;
                existingEntry.PrimaryMoodId = inputModel.PrimaryMoodId;
                existingEntry.CategoryId = inputModel.CategoryId;
                existingEntry.UpdatedAt = DateTime.UtcNow;

                // Remove existing secondary moods
                _context.JournalEntryMoods.RemoveRange(existingEntry.SecondaryMoods);
                
                // Add new secondary moods
                foreach (var moodId in inputModel.SecondaryMoodIds.Take(2)) // Max 2 secondary moods
                {
                    existingEntry.SecondaryMoods.Add(new JournalEntryMood
                    {
                        MoodId = moodId
                    });
                }

                // Remove existing tags
                _context.JournalEntryTags.RemoveRange(existingEntry.Tags);
                
                // Add existing tags
                foreach (var tagId in inputModel.TagIds)
                {
                    existingEntry.Tags.Add(new JournalEntryTag
                    {
                        TagId = tagId
                    });
                }

                // Add new tags
                foreach (var tagName in inputModel.NewTagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var existingTag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

                    if (existingTag == null)
                    {
                        var newTag = new Tag
                        {
                            Name = tagName.Trim(),
                            IsPreDefined = false,
                            Color = "#6C5CE7" // Default color for custom tags
                        };
                        _context.Tags.Add(newTag);
                        await _context.SaveChangesAsync();

                        existingEntry.Tags.Add(new JournalEntryTag
                        {
                            TagId = newTag.Id
                        });
                    }
                    else
                    {
                        existingEntry.Tags.Add(new JournalEntryTag
                        {
                            TagId = existingTag.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Load related data for display
                var updatedEntry = await _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .FirstAsync(je => je.Id == existingEntry.Id);

                var displayModel = MapToDisplayModel(updatedEntry);

                return ServiceResult<JournalEntryDisplayModel>.Ok(displayModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<JournalEntryDisplayModel>.Fail($"Error updating journal entry: {ex.Message}");
            }
        }

        public async Task<ServiceResult<JournalEntryDisplayModel>> GetEntryByDateAsync(DateTime date)
        {
            try
            {
                var entry = await _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .FirstOrDefaultAsync(je => je.Date.Date == date.Date);

                if (entry == null)
                {
                    return ServiceResult<JournalEntryDisplayModel>.Fail("No entry found for this date.");
                }

                var displayModel = MapToDisplayModel(entry);
                return ServiceResult<JournalEntryDisplayModel>.Ok(displayModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<JournalEntryDisplayModel>.Fail($"Error retrieving journal entry: {ex.Message}");
            }
        }

        public async Task<ServiceResult<JournalEntryDisplayModel>> GetEntryByIdAsync(int id)
        {
            try
            {
                var entry = await _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .FirstOrDefaultAsync(je => je.Id == id);

                if (entry == null)
                {
                    return ServiceResult<JournalEntryDisplayModel>.Fail("No entry found with this ID.");
                }

                var displayModel = MapToDisplayModel(entry);
                return ServiceResult<JournalEntryDisplayModel>.Ok(displayModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<JournalEntryDisplayModel>.Fail($"Error retrieving journal entry: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteEntryAsync(int id)
        {
            try
            {
                var entry = await _context.JournalEntries
                    .Include(je => je.SecondaryMoods)
                    .Include(je => je.Tags)
                    .FirstOrDefaultAsync(je => je.Id == id);

                if (entry == null)
                {
                    return ServiceResult<bool>.Fail("Entry not found.");
                }

                // Remove related entities
                _context.JournalEntryMoods.RemoveRange(entry.SecondaryMoods);
                _context.JournalEntryTags.RemoveRange(entry.Tags);

                // Remove the entry
                _context.JournalEntries.Remove(entry);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail($"Error deleting entry: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<Mood>>> GetMoodsAsync()
        {
            try
            {
                var moods = await _context.Moods
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .ToListAsync();

                return ServiceResult<List<Mood>>.Ok(moods);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Mood>>.Fail($"Error retrieving moods: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<Tag>>> GetTagsAsync()
        {
            try
            {
                var tags = await _context.Tags
                    .OrderBy(t => t.IsPreDefined ? 0 : 1) // Pre-defined tags first
                    .ThenBy(t => t.Name)
                    .ToListAsync();

                return ServiceResult<List<Tag>>.Ok(tags);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Tag>>.Fail($"Error retrieving tags: {ex.Message}");
            }
        }

        public async Task<ServiceResult<AnalyticsDisplayModel>> GetAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var entries = await _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .Include(je => je.Category)
                    .Where(je => je.Date >= startDate.Date && je.Date <= endDate.Date)
                    .OrderByDescending(je => je.Date)
                    .ToListAsync();

                if (!entries.Any())
                {
                    return ServiceResult<AnalyticsDisplayModel>.Ok(new AnalyticsDisplayModel
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    });
                }

                var analytics = new AnalyticsDisplayModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalEntries = entries.Count,
                    TotalWords = entries.Sum(e => StripHtmlTags(e.Content).Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length),
                    AverageWordsPerEntry = entries.Any() ? 
                        entries.Average(e => StripHtmlTags(e.Content).Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length) : 0,
                    CurrentStreak = 0, // Will be calculated later
                    LongestStreak = 0, // Will be calculated later
                    EntriesThisWeek = entries.Count(e => e.Date >= DateTime.Today.AddDays(-7)),
                    EntriesThisMonth = entries.Count(e => e.Date.Year == DateTime.Today.Year && e.Date.Month == DateTime.Today.Month),
                    MostProductiveDayOfWeek = GetMostProductiveDayOfWeek(entries),
                    LongestEntryWords = entries.Any() ? entries.Max(e => StripHtmlTags(e.Content).Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length) : 0,
                    ShortestEntryWords = entries.Any() ? entries.Min(e => StripHtmlTags(e.Content).Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length) : 0,
                    UniqueTagsCount = entries.SelectMany(e => e.Tags).Select(t => t.Tag.Name).Distinct().Count()
                };

                // Mood distribution
                analytics.MoodDistribution = CalculateMoodDistribution(entries);
                analytics.MostFrequentMood = analytics.MoodDistribution.OrderByDescending(m => m.Count).FirstOrDefault()?.Name ?? "";

                // Top tags
                analytics.TopTags = CalculateTopTags(entries);

                // Word count trend
                analytics.WordCountTrend = CalculateWordCountTrend(entries);

                return ServiceResult<AnalyticsDisplayModel>.Ok(analytics);
            }
            catch (Exception ex)
            {
                return ServiceResult<AnalyticsDisplayModel>.Fail($"Error retrieving analytics: {ex.Message}");
            }
        }

        private List<MoodFrequencyDisplayModel> CalculateMoodDistribution(List<JournalEntry> entries)
        {
            var moodCounts = new Dictionary<(int Id, string Name, string? Icon, MoodCategory Category), int>();

            foreach (var entry in entries)
            {
                if (entry.PrimaryMood != null)
                {
                    var key = (entry.PrimaryMood.Id, entry.PrimaryMood.Name, entry.PrimaryMood.Icon ?? "", entry.PrimaryMood.Category);
                    moodCounts[key] = moodCounts.GetValueOrDefault(key, 0) + 1;
                }
            }

            var total = moodCounts.Values.Sum();
            return moodCounts.Select(kvp => new MoodFrequencyDisplayModel
            {
                Name = kvp.Key.Name,
                Icon = kvp.Key.Icon ?? "",
                Category = kvp.Key.Category,
                Count = kvp.Value,
                Percentage = total > 0 ? (kvp.Value * 100.0 / total) : 0
            }).OrderByDescending(m => m.Count).ToList();
        }

        private List<TagFrequencyDisplayModel> CalculateTopTags(List<JournalEntry> entries)
        {
            var tagCounts = entries
                .SelectMany(e => e.Tags)
                .GroupBy(jet => jet.Tag.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(t => t.Count)
                .ToList();

            var total = tagCounts.Sum(t => t.Count);
            return tagCounts.Select(t => new TagFrequencyDisplayModel
            {
                Name = t.Name,
                Count = t.Count,
                Percentage = total > 0 ? (t.Count * 100.0 / total) : 0
            }).ToList();
        }

        private List<WordCountTrendDisplayModel> CalculateWordCountTrend(List<JournalEntry> entries)
        {
            return entries
                .OrderByDescending(e => e.Date)
                .Take(30) // Last 30 entries
                .Select(e => new WordCountTrendDisplayModel
                {
                    Date = e.Date,
                    WordCount = StripHtmlTags(e.Content).Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length,
                    FormattedDate = e.Date.ToString("MMM dd"),
                    PrimaryMood = e.PrimaryMood != null ? new MoodDisplayModel
                    {
                        Id = e.PrimaryMood.Id,
                        Name = e.PrimaryMood.Name,
                        Category = e.PrimaryMood.Category,
                        Icon = e.PrimaryMood.Icon,
                        Color = e.PrimaryMood.Color
                    } : null
                })
                .OrderBy(t => t.Date)
                .ToList();
        }

        private string GetMostProductiveDayOfWeek(List<JournalEntry> entries)
        {
            if (!entries.Any()) return "None";

            var dayGroups = entries
                .GroupBy(e => e.Date.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefault();

            return dayGroups?.Day.ToString() ?? "None";
        }

        public async Task<ServiceResult<List<JournalEntryDisplayModel>>> GetEntriesAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10)
        {
            try
            {
                var entries = await _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .Where(je => je.Date >= startDate.Date && je.Date <= endDate.Date)
                    .OrderByDescending(je => je.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var displayModels = entries.Select(MapToDisplayModel).ToList();
                return ServiceResult<List<JournalEntryDisplayModel>>.Ok(displayModels);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<JournalEntryDisplayModel>>.Fail($"Error retrieving journal entries: {ex.Message}");
            }
        }

        public async Task<ServiceResult<(List<JournalEntryDisplayModel> Entries, int TotalCount)>> GetEntriesWithCountAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .Where(je => je.Date >= startDate.Date && je.Date <= endDate.Date)
                    .OrderByDescending(je => je.Date);

                var totalCount = await query.CountAsync();
                
                var entries = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var displayModels = entries.Select(MapToDisplayModel).ToList();
                
                return ServiceResult<(List<JournalEntryDisplayModel>, int)>.Ok((displayModels, totalCount));
            }
            catch (Exception ex)
            {
                return ServiceResult<(List<JournalEntryDisplayModel>, int)>.Fail($"Error retrieving journal entries: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<JournalEntryDisplayModel>>> GetEntriesByMonthAsync(int year, int month)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                var entries = await _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .Where(je => je.Date >= startDate && je.Date <= endDate)
                    .OrderByDescending(je => je.Date)
                    .ToListAsync();

                var displayModels = entries.Select(MapToDisplayModel).ToList();
                return ServiceResult<List<JournalEntryDisplayModel>>.Ok(displayModels);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<JournalEntryDisplayModel>>.Fail($"Error retrieving journal entries for month: {ex.Message}");
            }
        }

        public async Task<ServiceResult<(List<JournalEntryDisplayModel> Entries, int TotalCount)>> SearchEntriesAsync(SearchInputModel searchModel)
        {
            try
            {
                var query = _context.JournalEntries
                    .Include(je => je.PrimaryMood)
                    .Include(je => je.Category)
                    .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                    .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                    .AsQueryable();

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchModel.Query))
                {
                    var searchTerm = searchModel.Query.Trim().ToLower();
                    query = query.Where(je => 
                        je.Content.ToLower().Contains(searchTerm));
                }

                if (searchModel.StartDate.HasValue)
                {
                    query = query.Where(je => je.Date.Date >= searchModel.StartDate.Value.Date);
                }

                if (searchModel.EndDate.HasValue)
                {
                    query = query.Where(je => je.Date.Date <= searchModel.EndDate.Value.Date);
                }

                if (searchModel.PrimaryMoodId.HasValue)
                {
                    query = query.Where(je => je.PrimaryMoodId == searchModel.PrimaryMoodId.Value);
                }

                if (searchModel.CategoryId.HasValue)
                {
                    query = query.Where(je => je.CategoryId == searchModel.CategoryId.Value);
                }

                if (searchModel.TagId.HasValue)
                {
                    query = query.Where(je => je.Tags.Any(jet => jet.TagId == searchModel.TagId.Value));
                }

                // Order by date (newest first)
                query = query.OrderByDescending(je => je.Date);

                var totalCount = await query.CountAsync();

                var entries = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .ToListAsync();

                var displayModels = entries.Select(MapToDisplayModel).ToList();

                return ServiceResult<(List<JournalEntryDisplayModel>, int)>.Ok((displayModels, totalCount));
            }
            catch (Exception ex)
            {
                return ServiceResult<(List<JournalEntryDisplayModel>, int)>.Fail($"Error searching entries: {ex.Message}");
            }
        }

        private JournalEntryDisplayModel MapToDisplayModel(JournalEntry entry)
        {
            return new JournalEntryDisplayModel
            {
                Id = entry.Id,
                Date = entry.Date,
                Content = entry.Content,
                ContentPreview = entry.Content.Length > 200 ? entry.Content.Substring(0, 200) + "..." : entry.Content,
                PlainTextPreview = StripHtmlTags(entry.Content).Length > 150 ? 
                    StripHtmlTags(entry.Content).Substring(0, 150) + "..." : 
                    StripHtmlTags(entry.Content),
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt,
                PrimaryMood = entry.PrimaryMood != null ? new MoodDisplayModel
                {
                    Id = entry.PrimaryMood.Id,
                    Name = entry.PrimaryMood.Name,
                    Category = entry.PrimaryMood.Category,
                    Icon = entry.PrimaryMood.Icon,
                    Color = entry.PrimaryMood.Color
                } : null,
                SecondaryMoods = entry.SecondaryMoods.Select(jem => new MoodDisplayModel
                {
                    Id = jem.Mood.Id,
                    Name = jem.Mood.Name,
                    Category = jem.Mood.Category,
                    Icon = jem.Mood.Icon,
                    Color = jem.Mood.Color
                }).ToList(),
                Category = entry.Category != null ? new CategoryDisplayModel
                {
                    Id = entry.Category.Id,
                    Name = entry.Category.Name,
                    Description = entry.Category.Description,
                    Color = entry.Category.Color
                } : null,
                Tags = entry.Tags.Select(jet => new TagDisplayModel
                {
                    Id = jet.Tag.Id,
                    Name = jet.Tag.Name,
                    IsPreDefined = jet.Tag.IsPreDefined,
                    Color = jet.Tag.Color
                }).ToList(),
                FormattedDate = entry.Date.ToString("MMMM dd, yyyy"),
                WordCount = entry.Content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length.ToString()
            };
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Remove HTML tags
            var stripped = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
            
            // Decode HTML entities
            stripped = System.Web.HttpUtility.HtmlDecode(stripped);
            
            // Clean up multiple whitespace and trim
            stripped = System.Text.RegularExpressions.Regex.Replace(stripped, @"\s+", " ").Trim();
            
            return stripped;
        }
    }
}
