using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly JournalService _journalService;

        [ObservableProperty]
        private string _greeting = "Hello!";

        [ObservableProperty]
        private string _todayStatus = "You haven't journaled today.";

        [ObservableProperty]
        private string _todayButtonText = "Create Today's Entry";

        public ObservableCollection<JournalEntry> RecentEntries { get; } = new();

        public DashboardViewModel(JournalService journalService)
        {
            _journalService = journalService;
        }

        [RelayCommand]
        public async Task Appearing()
        {
            UpdateGreeting();
            await CheckTodayEntry();
            await LoadRecentEntries();
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            Greeting = hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
        }

        private async Task CheckTodayEntry()
        {
            var todayEntry = await _journalService.GetEntryForDateAsync(DateTime.Today);
            if (todayEntry != null)
            {
                TodayStatus = "You have journaled today!";
                TodayButtonText = "Edit Today's Entry";
            }
            else
            {
                TodayStatus = "You haven't journaled today.";
                TodayButtonText = "Create Today's Entry";
            }
        }

        private async Task LoadRecentEntries()
        {
            RecentEntries.Clear();
            var entries = await _journalService.GetAllEntriesAsync();
            foreach (var entry in entries.Take(5))
            {
                RecentEntries.Add(entry);
            }
        }

        [RelayCommand]
        public async Task OpenTodayEntry()
        {
            var todayEntry = await _journalService.CreateEntryForTodayAsync();
            
            var navigationParameter = new Dictionary<string, object>
            {
                { "Id", todayEntry.Id } // Pass Id to editor
            };
            await Shell.Current.GoToAsync("entry_editor", navigationParameter);
        }

        [RelayCommand]
        public async Task OpenEntry(JournalEntry entry)
        {
             var navigationParameter = new Dictionary<string, object>
            {
                { "Id", entry.Id }
            };
            await Shell.Current.GoToAsync("entry_editor", navigationParameter);
        }
    }
}
