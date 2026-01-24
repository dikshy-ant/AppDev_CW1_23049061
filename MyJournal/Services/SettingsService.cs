using Microsoft.EntityFrameworkCore;
using MyJournal.Data;
using MyJournal.Entities;
using System.Security.Cryptography;
using System.Text;

namespace MyJournal.Services
{
    public class SettingsService
    {
        private readonly AppDbContext _context;

        public SettingsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserSettings> GetUserSettingsAsync()
        {
            var settings = await _context.UserSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // Create default settings if they don't exist
                settings = new UserSettings 
                { 
                    Id = 1,
                    Theme = "Light",
                    IsPasswordEnabled = false,
                    IsPinEnabled = false
                };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task<bool> UpdateThemeAsync(string theme)
        {
            try
            {
                var settings = await GetUserSettingsAsync();
                settings.Theme = theme;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePasswordSettingsAsync(bool isPasswordEnabled, string? passwordHash = null)
        {
            try
            {
                var settings = await GetUserSettingsAsync();
                settings.IsPasswordEnabled = isPasswordEnabled;
                if (passwordHash != null)
                {
                    settings.PasswordHash = passwordHash;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePinSettingsAsync(bool isPinEnabled, string? pinHash = null)
        {
            try
            {
                var settings = await GetUserSettingsAsync();
                settings.IsPinEnabled = isPinEnabled;
                if (pinHash != null)
                {
                    settings.PinHash = pinHash;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyPinAsync(string pin)
        {
            try
            {
                var settings = await GetUserSettingsAsync();
                if (!settings.IsPinEnabled || string.IsNullOrEmpty(settings.PinHash))
                {
                    // Debug: PIN not enabled
                    return false;
                }

                var hashedPin = HashPin(pin);
                var isValid = hashedPin == settings.PinHash;
                
                // Debug: Log the comparison (remove this in production)
                System.Diagnostics.Debug.WriteLine($"Input PIN: {pin}");
                System.Diagnostics.Debug.WriteLine($"Hashed Input: {hashedPin}");
                System.Diagnostics.Debug.WriteLine($"Stored Hash: {settings.PinHash}");
                System.Diagnostics.Debug.WriteLine($"Match: {isValid}");
                
                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PIN verification error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsPinEnabledAsync()
        {
            try
            {
                var settings = await GetUserSettingsAsync();
                return settings.IsPinEnabled && !string.IsNullOrEmpty(settings.PinHash);
            }
            catch
            {
                return false;
            }
        }

        private string HashPin(string pin)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin + "journal_salt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
