using System.ComponentModel.DataAnnotations;
using MyJournal.Entities;

namespace MyJournal.Models.Input
{
    public class SearchInputModel
    {
        public string? Query { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public int? PrimaryMoodId { get; set; }
        
        public MoodCategory? MoodCategory { get; set; }
        
        public int? CategoryId { get; set; }
        
        public int? TagId { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
    }
}
