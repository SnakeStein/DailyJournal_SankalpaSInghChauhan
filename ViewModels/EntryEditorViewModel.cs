using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Models;
using SecureDailyJournal.Services;
using Markdig;

namespace SecureDailyJournal.ViewModels
{
    [QueryProperty(nameof(EntryId), "Id")]
    public partial class EntryEditorViewModel : ObservableObject
    {
        private readonly JournalService _journalService;
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private int _entryId;

        [ObservableProperty]
        private string _title = "";

        [ObservableProperty]
        private string _content = "";

        [ObservableProperty]
        private string _htmlPreview = "";

        [ObservableProperty]
        private DateTime _entryDate;

        [ObservableProperty]
        private string _selectedMoodName;
        
        [ObservableProperty]
        private List<string> _moodValues = new();

        private JournalEntry _currentEntry;

        public EntryEditorViewModel(JournalService journalService, DatabaseService databaseService)
        {
            _journalService = journalService;
            _databaseService = databaseService;
            LoadMoods();
        }

        private async void LoadMoods()
        {
            var moods = await _databaseService.GetMoodsAsync();
            MoodValues = moods.Select(m => m.Name).ToList();
        }

        async partial void OnEntryIdChanged(int value)
        {
            if (value > 0)
            {
                _currentEntry = await _journalService.GetEntryForDateAsync(DateTime.Today); // Wait, ID isn't Date.
                // Assuming ID is passed if editing history, or we fetch by ID.
                // Re-fetch by ID logic:
                _currentEntry = await _databaseService.GetEntryAsync(value);
            }
            // If value is 0, we might be creating new, but usually we pass an ID or handle "New" efficiently. 
            // For now let's assume we load if ID is present.
            
            if (_currentEntry != null)
            {
                LoadEntry(_currentEntry);
            }
        }

        // Method to initialize with a specific entry (e.g. passed from Dashboard)
        public void Initialize(JournalEntry entry)
        {
            _currentEntry = entry;
            LoadEntry(entry);
        }

        private void LoadEntry(JournalEntry entry)
        {
            Title = entry.Title;
            Content = entry.Content;
            EntryDate = entry.EntryDate;
            SelectedMoodName = entry.PrimaryMood;
            UpdatePreview();
        }

        partial void OnContentChanged(string value)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            HtmlPreview = Markdown.ToHtml(Content ?? "", pipeline);
        }

        [RelayCommand]
        public async Task Save()
        {
            if (_currentEntry == null) return;

            _currentEntry.Title = Title;
            _currentEntry.Content = Content;
            _currentEntry.PrimaryMood = SelectedMoodName;
            
            await _journalService.SaveEntryAsync(_currentEntry);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
