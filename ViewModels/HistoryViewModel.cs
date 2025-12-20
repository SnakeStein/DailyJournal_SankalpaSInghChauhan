using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly JournalService _journalService;
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today.AddMonths(-3);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private string _selectedMood = "All";

        public ObservableCollection<string> MoodOptions { get; } = new() { "All" };
        public ObservableCollection<JournalEntry> FilteredEntries { get; } = new();

        private List<JournalEntry> _allEntries = new();

        public HistoryViewModel(JournalService journalService, DatabaseService databaseService)
        {
            _journalService = journalService;
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task Appearing()
        {
            await LoadMoods();
            await LoadEntries();
        }

        private async Task LoadMoods()
        {
            MoodOptions.Clear();
            MoodOptions.Add("All");
            var moods = await _databaseService.GetMoodsAsync();
            foreach (var m in moods) MoodOptions.Add(m.Name);
        }

        [RelayCommand]
        public async Task LoadEntries()
        {
            _allEntries = await _journalService.GetAllEntriesAsync();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var query = _allEntries.AsEnumerable();

            // Date Range
            query = query.Where(e => e.EntryDate.Date >= StartDate.Date && e.EntryDate.Date <= EndDate.Date);

            // Search Text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                query = query.Where(e => (e.Title?.ToLower().Contains(search) ?? false) || 
                                         (e.Content?.ToLower().Contains(search) ?? false));
            }

            // Mood
            if (SelectedMood != "All")
            {
                query = query.Where(e => e.PrimaryMood == SelectedMood);
            }

            FilteredEntries.Clear();
            foreach (var entry in query.OrderByDescending(e => e.EntryDate))
            {
                FilteredEntries.Add(entry);
            }
        }

        // Trigger filter on any property change
        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnStartDateChanged(DateTime value) => ApplyFilters();
        partial void OnEndDateChanged(DateTime value) => ApplyFilters();
        partial void OnSelectedMoodChanged(string value) => ApplyFilters();

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
