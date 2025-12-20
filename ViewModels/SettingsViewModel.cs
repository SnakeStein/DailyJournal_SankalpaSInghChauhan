using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Services;

namespace SecureDailyJournal.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly PdfExportService _pdfExportService;
        private readonly JournalService _journalService;

        [ObservableProperty]
        private bool _isDarkTheme;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        public SettingsViewModel(PdfExportService pdfExportService, JournalService journalService)
        {
            _pdfExportService = pdfExportService;
            _journalService = journalService;
            IsDarkTheme = Application.Current?.UserAppTheme == AppTheme.Dark;
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
        }

        [RelayCommand]
        public async Task ExportPdf()
        {
            if (StartDate > EndDate)
            {
                await Shell.Current.DisplayAlert("Error", "Start date cannot be after End date", "OK");
                return;
            }

            var entries = await _journalService.GetAllEntriesAsync(); 
            // Better to filter in Service, but List is okay for now or use Linq
            var filtered = entries.Where(e => e.EntryDate.Date >= StartDate.Date && e.EntryDate.Date <= EndDate.Date).OrderBy(e => e.EntryDate).ToList();

            if (!filtered.Any())
            {
                await Shell.Current.DisplayAlert("Info", "No entries found in selected range", "OK");
                return;
            }

            try
            {
                var path = await _pdfExportService.ExportToPdfAsync(filtered, StartDate, EndDate);
                await Shell.Current.DisplayAlert("Success", $"PDF saved to: {path}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
            }
        }
    }
}
