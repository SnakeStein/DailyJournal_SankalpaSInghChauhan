using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SecureDailyJournal.Services;
using System.Collections.ObjectModel;

namespace SecureDailyJournal.ViewModels
{
    public partial class AnalyticsViewModel : ObservableObject
    {
        private readonly AnalyticsService _analyticsService;

        [ObservableProperty]
        private int _currentStreak;

        [ObservableProperty]
        private int _longestStreak;

        [ObservableProperty]
        private int _totalWords;

        public ObservableCollection<ISeries> MoodSeries { get; set; } = new();

        public AnalyticsViewModel(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [RelayCommand]
        public async Task Appearing()
        {
            await LoadStats();
        }

        private async Task LoadStats()
        {
            var streaks = await _analyticsService.GetStreakStatsAsync();
            CurrentStreak = streaks.CurrentStreak;
            LongestStreak = streaks.LongestStreak;
            TotalWords = await _analyticsService.GetTotalWordsAsync();

            var moods = await _analyticsService.GetMoodDistributionAsync();
            MoodSeries.Clear();
            foreach (var mood in moods)
            {
                MoodSeries.Add(new PieSeries<int>
                {
                    Values = new int[] { mood.Value },
                    Name = mood.Key,
                    // Optionally set Color based on mood name
                });
            }
        }
    }
}
