using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public string DisplayText { get; set; } // Day number
        public bool IsCurrentMonth { get; set; }
        public bool HasEntry { get; set; }
        public string IndicatorColor { get; set; } = "Transparent";
    }

    public partial class CalendarViewModel : ObservableObject
    {
        private readonly JournalService _journalService;

        [ObservableProperty]
        private DateTime _currentMonth;

        [ObservableProperty]
        private string _monthName;

        public ObservableCollection<CalendarDay> Days { get; } = new();

        public CalendarViewModel(JournalService journalService)
        {
            _journalService = journalService;
            CurrentMonth = DateTime.Today;
            LoadMonth();
        }

        [RelayCommand]
        public void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            LoadMonth();
        }

        [RelayCommand]
        public void PrevMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            LoadMonth();
        }

        [RelayCommand]
        public async Task Appearing()
        {
            LoadMonth();
        }

        private async void LoadMonth()
        {
            MonthName = CurrentMonth.ToString("MMMM yyyy");
            Days.Clear();

            var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var offset = (int)firstDayOfMonth.DayOfWeek; // 0 = Sunday

            // Previous month padding
            for (int i = 0; i < offset; i++)
            {
                Days.Add(new CalendarDay { DisplayText = "", IsCurrentMonth = false });
            }

            // Current month
            var entries = await _journalService.GetEntriesForMonthAsync(CurrentMonth);
            
            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, i);
                var entry = entries.FirstOrDefault(e => e.EntryDate.Date == date);
                
                Days.Add(new CalendarDay 
                { 
                    Date = date,
                    DisplayText = i.ToString(), 
                    IsCurrentMonth = true,
                    HasEntry = entry != null,
                    IndicatorColor = entry != null ? "#3B82F6" : "Transparent" // Blue if entry exists
                });
            }
        }

        [RelayCommand]
        public async Task DayTapped(CalendarDay day)
        {
            if (!day.IsCurrentMonth) return;

            // Open Entry for that date (Create or View)
            // Navigate to Editor with Date query param maybe? 
            // Or fetched ID.

            var existing = await _journalService.GetEntryForDateAsync(day.Date);
            int id = existing?.Id ?? 0;

            if (id == 0)
            {
                // Logic to create specific date entry?
                // Requirement: "One journal entry per day".
                // If past date, can we create? Yes usually.Future? Maybe not.
                // Let's assume we can create entries for any past/present date.
                if (day.Date > DateTime.Today)
                {
                    await Shell.Current.DisplayAlert("Future Date", "You cannot journal for the future!", "OK");
                    return;
                }
                
                // We need to pass Date to Editor if creating new.
                // EditorViewModel needs to handle "New for Date".
                // Currently EditorViewModel handles "New for Today" via Initialize or just assumes created.
                // Let's pass DateParameter.
            }

             var navigationParameter = new Dictionary<string, object>
            {
                { "Id", id },
                { "Date", day.Date } // Need to support this in VM
            };
            await Shell.Current.GoToAsync("entry_editor", navigationParameter);
        }
    }
}
