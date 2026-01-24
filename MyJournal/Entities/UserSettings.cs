using System.ComponentModel.DataAnnotations;

namespace MyJournal.Entities
{
    public class UserSettings
    {
        public int Id { get; set; }
        
        [StringLength(20)]
        public string Theme { get; set; } = "Light";
        
        [StringLength(256)]
        public string? PasswordHash { get; set; }
        
        [StringLength(256)]
        public string? PinHash { get; set; }
        
        public bool IsPasswordEnabled { get; set; } = false;
        public bool IsPinEnabled { get; set; } = false;
    }
}
