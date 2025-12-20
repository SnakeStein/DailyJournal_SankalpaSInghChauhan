using SecureDailyJournal.Models;

namespace SecureDailyJournal.Services
{
    public class AnalyticsService
    {
        private readonly DatabaseService _databaseService;

        public AnalyticsService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<(int CurrentStreak, int LongestStreak)> GetStreakStatsAsync()
        {
            var entries = await _databaseService.GetEntriesAsync();
            if (!entries.Any()) return (0, 0);

            var sortedDates = entries.Select(e => e.EntryDate.Date).Distinct().OrderByDescending(d => d).ToList();

            int currentStreak = 0;
            int longestStreak = 0;
            int tempStreak = 0;

            // Check current streak
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // If we have entry today, start counting. If not, check yesterday. 
            // If neither, current streak is 0.
            if (sortedDates.Contains(today))
            {
                currentStreak = 1;
                var checkDate = today.AddDays(-1);
                while (sortedDates.Contains(checkDate))
                {
                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }
            }
            else if (sortedDates.Contains(yesterday))
            {
                currentStreak = 1;
                var checkDate = yesterday.AddDays(-1);
                while (sortedDates.Contains(checkDate))
                {
                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }
            }

            // Calculate longest streak
            // Iterate through dates and find consecutive sequence
            // Since sorted descending:
            // 2023-10-05, 2023-10-04, 2023-10-02
            
            if (sortedDates.Any())
            {
                // To simplify, let's sort ascending for longest calculation
                var ascDates = sortedDates.OrderBy(d => d).ToList();
                tempStreak = 1;
                longestStreak = 1;

                for (int i = 1; i < ascDates.Count; i++)
                {
                    if (ascDates[i] == ascDates[i - 1].AddDays(1))
                    {
                        tempStreak++;
                    }
                    else
                    {
                        tempStreak = 1;
                    }
                    if (tempStreak > longestStreak) longestStreak = tempStreak;
                }
            }

            return (currentStreak, longestStreak);
        }

        public async Task<Dictionary<string, int>> GetMoodDistributionAsync()
        {
            var entries = await _databaseService.GetEntriesAsync();
            var stats = new Dictionary<string, int>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.PrimaryMood)) continue;
                if (stats.ContainsKey(entry.PrimaryMood))
                    stats[entry.PrimaryMood]++;
                else
                    stats[entry.PrimaryMood] = 1;
            }
            return stats;
        }
        
        public async Task<int> GetTotalWordsAsync()
        {
             var entries = await _databaseService.GetEntriesAsync();
             return entries.Sum(e => (e.Content ?? "").Split(new[]{' ', '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).Length);
        }
    }
}
