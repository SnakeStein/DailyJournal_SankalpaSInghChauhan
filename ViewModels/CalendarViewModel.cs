using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SecureDailyJournal.ViewModels
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool HasEntry { get; set; }
        public Color BackgroundColor { get; set; } = Colors.Transparent;
        public Color TextColor { get; set; } = Colors.Black;
    }

    public partial class CalendarViewModel : ObservableObject
    {
        private readonly JournalService _journalService;

        [ObservableProperty]
        private string _monthName = string.Empty;

        [ObservableProperty]
        private DateTime _currentMonth;

        public ObservableCollection<CalendarDay> Days { get; } = new();

        public CalendarViewModel(JournalService journalService)
        {
            _journalService = journalService;
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        [RelayCommand]
        public async Task Appearing()
        {
            await LoadMonth();
        }

        [RelayCommand]
        public async Task PrevMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            await LoadMonth();
        }

        [RelayCommand]
        public async Task NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            await LoadMonth();
        }

        private async Task LoadMonth()
        {
            Days.Clear();
            MonthName = CurrentMonth.ToString("MMMM yyyy");

            int daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            var entries = await _journalService.GetEntriesForMonthAsync(CurrentMonth);

            // Add empty days before the first day of the month
            for (int i = 0; i < startDayOfWeek; i++)
            {
                Days.Add(new CalendarDay { IsCurrentMonth = false });
            }

            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, i);
                var entry = entries.FirstOrDefault(e => e.EntryDate.Date == date);
                var isToday = date == DateTime.Today;

                Color bg = Colors.Transparent;
                Color text = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

                if (entry != null)
                {
                    bg = Color.FromArgb("#3B82F6"); // Blue for creation
                    text = Colors.White;
                }
                else if (isToday)
                {
                    bg = Color.FromArgb("#DBEAFE"); // Light Blue for Today
                    text = Color.FromArgb("#1D4ED8"); // Dark Blue text
                }

                Days.Add(new CalendarDay
                {
                    Date = date,
                    DisplayText = i.ToString(),
                    IsCurrentMonth = true,
                    IsToday = isToday,
                    HasEntry = entry != null,
                    BackgroundColor = bg,
                    TextColor = text
                });
            }
        }

        [RelayCommand]
        public async Task DayTapped(CalendarDay day)
        {
            if (!day.IsCurrentMonth) return;

            var existing = await _journalService.GetEntryForDateAsync(day.Date);
            int id = existing?.Id ?? 0;

            if (id == 0)
            {
                if (day.Date > DateTime.Today)
                {
                    await Shell.Current.DisplayAlert("Future Date", "You cannot journal for the future!", "OK");
                    return;
                }
            }

            var navigationParameter = new Dictionary<string, object>
            {
                { "Id", id },
                { "Date", day.Date }
            };
            await Shell.Current.GoToAsync("entry_editor", navigationParameter);
        }
    }
}
