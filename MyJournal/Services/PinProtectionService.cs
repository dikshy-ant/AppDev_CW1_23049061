using Microsoft.AspNetCore.Components;
using MyJournal.Services;
using MyJournal.Entities;

namespace MyJournal.Services
{
    public class PinProtectionService
    {
        private readonly SettingsService _settingsService;
        private readonly NavigationManager _navigationManager;
        
        // Static variable to track authentication state (works better in MAUI)
        private static bool _isAuthenticated = false;

        public PinProtectionService(SettingsService settingsService, NavigationManager navigationManager)
        {
            _settingsService = settingsService;
            _navigationManager = navigationManager;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                // Check if PIN is enabled
                var isPinEnabled = await _settingsService.IsPinEnabledAsync();
                if (!isPinEnabled)
                    return true; // No PIN protection needed

                // Check static authentication state
                System.Diagnostics.Debug.WriteLine($"IsAuthenticated check: {_isAuthenticated}");
                return _isAuthenticated;
            }
            catch
            {
                return false;
            }
        }

        public async Task SetAuthenticatedStateAsync(bool isAuthenticated)
        {
            _isAuthenticated = isAuthenticated;
            System.Diagnostics.Debug.WriteLine($"SetAuthenticatedState: {isAuthenticated}");
            await Task.CompletedTask; // Make it async for interface compatibility
        }

        public async Task<bool> IsPinEnabledAsync()
        {
            try
            {
                return await _settingsService.IsPinEnabledAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task RequireAuthenticationAsync()
        {
            var isPinEnabled = await _settingsService.IsPinEnabledAsync();
            System.Diagnostics.Debug.WriteLine($"RequireAuthentication: PIN enabled = {isPinEnabled}");
            
            if (!isPinEnabled)
                return; // No PIN protection needed

            var isAuthenticated = await IsAuthenticatedAsync();
            System.Diagnostics.Debug.WriteLine($"RequireAuthentication: Is authenticated = {isAuthenticated}");
            
            if (!isAuthenticated)
            {
                // Redirect to PIN verification page
                System.Diagnostics.Debug.WriteLine("Redirecting to PIN verification");
                _navigationManager.NavigateTo("/pin-verify", forceLoad: true);
            }
        }

        public async Task LogoutAsync()
        {
            _isAuthenticated = false;
            _navigationManager.NavigateTo("/pin-verify");
            await Task.CompletedTask;
        }
    }
}
