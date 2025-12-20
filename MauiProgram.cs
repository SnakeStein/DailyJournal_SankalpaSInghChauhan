using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SecureDailyJournal;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
            .UseSkiaSharp();

        // Services
        builder.Services.AddSingleton<Services.DatabaseService>();
        builder.Services.AddSingleton<Services.SecurityService>();
        builder.Services.AddSingleton<Services.JournalService>();
        builder.Services.AddSingleton<Services.AnalyticsService>();
        builder.Services.AddSingleton<Services.PdfExportService>();
        
        // ViewModels
        builder.Services.AddTransient<ViewModels.LoginViewModel>();
        builder.Services.AddTransient<ViewModels.EntryEditorViewModel>();
        builder.Services.AddTransient<ViewModels.DashboardViewModel>();
        builder.Services.AddTransient<ViewModels.CalendarViewModel>();
        builder.Services.AddTransient<ViewModels.AnalyticsViewModel>();
        builder.Services.AddTransient<ViewModels.SettingsViewModel>();
        builder.Services.AddTransient<ViewModels.HistoryViewModel>();

        // Pages
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<Views.EntryEditorPage>();
        builder.Services.AddTransient<Views.DashboardPage>();
        builder.Services.AddTransient<Views.CalendarPage>();
        builder.Services.AddTransient<Views.AnalyticsPage>();
        builder.Services.AddTransient<Views.SettingsPage>();
        builder.Services.AddTransient<Views.HistoryPage>();
        builder.Services.AddTransient<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
