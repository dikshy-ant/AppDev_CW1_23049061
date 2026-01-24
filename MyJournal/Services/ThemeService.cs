using Microsoft.JSInterop;
using MudBlazor;

namespace MyJournal.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly SettingsService _settingsService;

        public event Action<MudTheme>? ThemeChanged;

        public MudTheme CurrentTheme { get; private set; }

        public ThemeService(IJSRuntime jsRuntime, SettingsService settingsService)
        {
            _jsRuntime = jsRuntime;
            _settingsService = settingsService;
            
            // Initialize with default light theme
            CurrentTheme = new MudTheme();
        }

        public async Task InitializeThemeAsync()
        {
            try
            {
                // Get user's saved theme preference
                var settings = await _settingsService.GetUserSettingsAsync();
                var savedTheme = settings.Theme ?? "Light";

                // Apply the theme
                await SetThemeAsync(savedTheme);
            }
            catch
            {
                // Fallback to light theme
                await SetThemeAsync("Light");
            }
        }

        public async Task SetThemeAsync(string themeName)
        {
            MudTheme newTheme;

            switch (themeName.ToLower())
            {
                case "dark":
                    newTheme = new MudTheme() { PaletteLight = new PaletteLight(), PaletteDark = new PaletteDark() };
                    break;
                default:
                    newTheme = new MudTheme() { PaletteLight = new PaletteLight(), PaletteDark = new PaletteDark() };
                    break;
            }

            CurrentTheme = newTheme;
            
            // Save preference
            await _settingsService.UpdateThemeAsync(themeName);
            
            // Notify listeners
            ThemeChanged?.Invoke(newTheme);
        }
    }
}
