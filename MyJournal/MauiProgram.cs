using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using MyJournal.Data;
using MyJournal.Services;

namespace MyJournal
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

            // Add DbContext
            builder.Services.AddDbContext<AppDbContext>();

            // Register JournalService
            builder.Services.AddScoped<JournalService>();
            
            // Register ExportService
            builder.Services.AddScoped<ExportService>();
            
            // Register StreakService
            builder.Services.AddScoped<StreakService>();
            
            // Register SettingsService
            builder.Services.AddScoped<SettingsService>();
            
            // Register PinProtectionService
            builder.Services.AddScoped<PinProtectionService>();
            
            // Register ThemeService as singleton
            builder.Services.AddSingleton<ThemeService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Ensure database is created
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            return app;
        }
    }
}
