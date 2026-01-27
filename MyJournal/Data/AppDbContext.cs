using Microsoft.EntityFrameworkCore;
using MyJournal.Entities;
using System.IO;

namespace MyJournal.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<JournalEntryMood> JournalEntryMoods { get; set; }
        public DbSet<JournalEntryTag> JournalEntryTags { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<StreakTracking> StreakTracking { get; set; }

        private readonly string _dbPath;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dbPath = Path.Combine(folder, "myjournal.db");
        }

        public AppDbContext()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dbPath = Path.Combine(folder, "myjournal.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationships
            modelBuilder.Entity<JournalEntryMood>()
                .HasKey(jem => new { jem.JournalEntryId, jem.MoodId });
            modelBuilder.Entity<JournalEntryMood>()
                .HasOne(jem => jem.JournalEntry)
                .WithMany(je => je.SecondaryMoods)
                .HasForeignKey(jem => jem.JournalEntryId);
            modelBuilder.Entity<JournalEntryMood>()
                .HasOne(jem => jem.Mood)
                .WithMany()
                .HasForeignKey(jem => jem.MoodId);

            modelBuilder.Entity<JournalEntryTag>()
                .HasKey(jet => new { jet.JournalEntryId, jet.TagId });
            modelBuilder.Entity<JournalEntryTag>()
                .HasOne(jet => jet.JournalEntry)
                .WithMany(je => je.Tags)
                .HasForeignKey(jet => jet.JournalEntryId);
            modelBuilder.Entity<JournalEntryTag>()
                .HasOne(jet => jet.Tag)
                .WithMany()
                .HasForeignKey(jet => jet.TagId);

            // Configure JournalEntry relationships
            modelBuilder.Entity<JournalEntry>()
                .HasOne(je => je.PrimaryMood)
                .WithMany()
                .HasForeignKey(je => je.PrimaryMoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(je => je.Category)
                .WithMany(c => c.JournalEntries)
                .HasForeignKey(je => je.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ensure one entry per day constraint
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(je => je.Date)
                .IsUnique();

            // Seed default moods
            modelBuilder.Entity<Mood>().HasData(
                // Positive moods
                new Mood { Id = 1, Name = "Happy", Category = MoodCategory.Positive, Icon = "@Icons.Material.Filled.SentimentVerySatisfied", Color = "#FFD700" },
                new Mood { Id = 2, Name = "Excited", Category = MoodCategory.Positive, Icon = "@Icons.Material.Filled.Celebration", Color = "#FF6B6B" },
                new Mood { Id = 3, Name = "Relaxed", Category = MoodCategory.Positive, Icon = "@Icons.Material.Filled.Spa", Color = "#87CEEB" },
                new Mood { Id = 4, Name = "Grateful", Category = MoodCategory.Positive, Icon = "@Icons.Material.Filled.VolunteerActivism", Color = "#98FB98" },
                new Mood { Id = 5, Name = "Confident", Category = MoodCategory.Positive, Icon = "@Icons.Material.Filled.FitnessCenter", Color = "#DDA0DD" },
                
                // Neutral moods
                new Mood { Id = 6, Name = "Calm", Category = MoodCategory.Neutral, Icon = "@Icons.Material.Filled.Water", Color = "#B0E0E6" },
                new Mood { Id = 7, Name = "Thoughtful", Category = MoodCategory.Neutral, Icon = "@Icons.Material.Filled.Psychology", Color = "#D3D3D3" },
                new Mood { Id = 8, Name = "Curious", Category = MoodCategory.Neutral, Icon = "@Icons.Material.Filled.TravelExplore", Color = "#F0E68C" },
                new Mood { Id = 9, Name = "Nostalgic", Category = MoodCategory.Neutral, Icon = "@Icons.Material.Filled.PhotoCamera", Color = "#DDA0DD" },
                new Mood { Id = 10, Name = "Bored", Category = MoodCategory.Neutral, Icon = "@Icons.Material.Filled.HourglassEmpty", Color = "#A9A9A9" },
                
                // Negative moods
                new Mood { Id = 11, Name = "Sad", Category = MoodCategory.Negative, Icon = "@Icons.Material.Filled.SentimentVeryDissatisfied", Color = "#4682B4" },
                new Mood { Id = 12, Name = "Angry", Category = MoodCategory.Negative, Icon = "@Icons.Material.Filled.MoodBad", Color = "#DC143C" },
                new Mood { Id = 13, Name = "Stressed", Category = MoodCategory.Negative, Icon = "@Icons.Material.Filled.PsychologyAlt", Color = "#FF8C00" },
                new Mood { Id = 14, Name = "Lonely", Category = MoodCategory.Negative, Icon = "@Icons.Material.Filled.PersonOff", Color = "#708090" },
                new Mood { Id = 15, Name = "Anxious", Category = MoodCategory.Negative, Icon = "@Icons.Material.Filled.HealthAndSafety", Color = "#9370DB" }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Personal", Description = "Personal thoughts and reflections", Color = "#FF6B6B" },
                new Category { Id = 2, Name = "Work", Description = "Work-related entries", Color = "#4ECDC4" },
                new Category { Id = 3, Name = "Health", Description = "Health and wellness", Color = "#45B7D1" }
            );

            modelBuilder.Entity<Tag>().HasData(
                new Tag { Id = 1, Name = "Work", IsPreDefined = true, Color = "#FF6B6B" },
                new Tag { Id = 2, Name = "Career", IsPreDefined = true, Color = "#4ECDC4" },
                new Tag { Id = 3, Name = "Studies", IsPreDefined = true, Color = "#45B7D1" },
                new Tag { Id = 4, Name = "Family", IsPreDefined = true, Color = "#96CEB4" },
                new Tag { Id = 5, Name = "Friends", IsPreDefined = true, Color = "#FFEAA7" },
                new Tag { Id = 6, Name = "Relationships", IsPreDefined = true, Color = "#DDA0DD" },
                new Tag { Id = 7, Name = "Health", IsPreDefined = true, Color = "#74B9FF" },
                new Tag { Id = 8, Name = "Fitness", IsPreDefined = true, Color = "#A29BFE" },
                new Tag { Id = 9, Name = "Personal Growth", IsPreDefined = true, Color = "#FD79A8" },
                new Tag { Id = 10, Name = "Self-care", IsPreDefined = true, Color = "#FDCB6E" },
                new Tag { Id = 11, Name = "Hobbies", IsPreDefined = true, Color = "#6C5CE7" },
                new Tag { Id = 12, Name = "Travel", IsPreDefined = true, Color = "#00B894" },
                new Tag { Id = 13, Name = "Nature", IsPreDefined = true, Color = "#00CEC9" },
                new Tag { Id = 14, Name = "Finance", IsPreDefined = true, Color = "#E17055" },
                new Tag { Id = 15, Name = "Spirituality", IsPreDefined = true, Color = "#0984E3" },
                new Tag { Id = 16, Name = "Birthday", IsPreDefined = true, Color = "#FDCB6E" },
                new Tag { Id = 17, Name = "Holiday", IsPreDefined = true, Color = "#E17055" },
                new Tag { Id = 18, Name = "Vacation", IsPreDefined = true, Color = "#00B894" },
                new Tag { Id = 19, Name = "Celebration", IsPreDefined = true, Color = "#FDCB6E" },
                new Tag { Id = 20, Name = "Exercise", IsPreDefined = true, Color = "#74B9FF" },
                new Tag { Id = 21, Name = "Reading", IsPreDefined = true, Color = "#A29BFE" },
                new Tag { Id = 22, Name = "Writing", IsPreDefined = true, Color = "#FD79A8" },
                new Tag { Id = 23, Name = "Cooking", IsPreDefined = true, Color = "#FDCB6E" },
                new Tag { Id = 24, Name = "Meditation", IsPreDefined = true, Color = "#6C5CE7" },
                new Tag { Id = 25, Name = "Yoga", IsPreDefined = true, Color = "#00B894" },
                new Tag { Id = 26, Name = "Music", IsPreDefined = true, Color = "#E17055" },
                new Tag { Id = 27, Name = "Shopping", IsPreDefined = true, Color = "#FDCB6E" },
                new Tag { Id = 28, Name = "Parenting", IsPreDefined = true, Color = "#74B9FF" },
                new Tag { Id = 29, Name = "Projects", IsPreDefined = true, Color = "#A29BFE" },
                new Tag { Id = 30, Name = "Planning", IsPreDefined = true, Color = "#FDCB6E" },
                new Tag { Id = 31, Name = "Reflection", IsPreDefined = true, Color = "#6C5CE7" }
            );

            modelBuilder.Entity<UserSettings>().HasData(
                new UserSettings { Id = 1, Theme = "Light", IsPasswordEnabled = false, IsPinEnabled = false }
            );

            modelBuilder.Entity<StreakTracking>().HasData(
                new StreakTracking { Id = 1, LastJournalDate = DateTime.MinValue, CurrentStreak = 0, LongestStreak = 0, MissedDaysJson = "[]" }
            );
        }
    }
}
