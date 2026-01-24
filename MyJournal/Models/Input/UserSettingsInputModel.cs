using System.ComponentModel.DataAnnotations;

namespace MyJournal.Models.Input
{
    public class UserSettingsInputModel
    {
        [Required]
        [StringLength(20, ErrorMessage = "Theme name cannot exceed 20 characters")]
        public string Theme { get; set; } = "Light";
        
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
        public string? NewPassword { get; set; }
        
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
        public string? ConfirmPassword { get; set; }
        
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN must be exactly 4 digits")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits")]
        public string? NewPin { get; set; }
        
        [Compare("NewPin", ErrorMessage = "PINs do not match")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits")]
        public string? ConfirmPin { get; set; }
        
        public bool IsPasswordEnabled { get; set; } = false;
        public bool IsPinEnabled { get; set; } = false;
    }
}
