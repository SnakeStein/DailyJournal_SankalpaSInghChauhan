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
        private string _secondaryMood1;

        [ObservableProperty]
        private string _secondaryMood2;
        
        [ObservableProperty]
        private List<string> _moodValues = new();

        [ObservableProperty]
        private string _newTag = "";

        // Simple string list for now, or could refer to Tag objects
        public ObservableCollection<string> SelectedTags { get; } = new();
        public ObservableCollection<string> AvailableTags { get; } = new();

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

            // Load Tags
            var tags = await _databaseService.GetAllTagsAsync(); // Need to implement
            AvailableTags.Clear();
            foreach (var t in tags) AvailableTags.Add(t.Name);
        }

        [RelayCommand]
        public async Task AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTag)) return;
            var tag = NewTag.Trim();
            if (!SelectedTags.Contains(tag))
            {
                SelectedTags.Add(tag);
                // Also ensure it exists in DB later or add to available
                if (!AvailableTags.Contains(tag)) AvailableTags.Add(tag);
            }
            NewTag = "";
        }

        [RelayCommand]
        public void RemoveTag(string tag)
        {
            if (SelectedTags.Contains(tag)) SelectedTags.Remove(tag);
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

        private async void LoadEntry(JournalEntry entry)
        {
            Title = entry.Title;
            Content = entry.Content;
            EntryDate = entry.EntryDate;
            SelectedMoodName = entry.PrimaryMood;
            
            // Parse secondary moods
            if (!string.IsNullOrEmpty(entry.SecondaryMoods))
            {
                var parts = entry.SecondaryMoods.Split(',');
                if (parts.Length > 0) SecondaryMood1 = parts[0];
                if (parts.Length > 1) SecondaryMood2 = parts[1];
            }
            else
            {
                SecondaryMood1 = null;
                SecondaryMood2 = null;
            }

            // Load tags for this entry
            SelectedTags.Clear();
            var tags = await _databaseService.GetTagsForEntryAsync(entry.Id); // Need implementation
            foreach (var t in tags) SelectedTags.Add(t.Name);

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
            
            var sec = new List<string>();
            if (!string.IsNullOrEmpty(SecondaryMood1)) sec.Add(SecondaryMood1);
            if (!string.IsNullOrEmpty(SecondaryMood2)) sec.Add(SecondaryMood2);
            _currentEntry.SecondaryMoods = string.Join(",", sec);

            await _journalService.SaveEntryAsync(_currentEntry);
            
            // Save Tags
            await _journalService.SaveTagsForEntryAsync(_currentEntry.Id, SelectedTags.ToList());

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
