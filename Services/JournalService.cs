using SecureDailyJournal.Models;

namespace SecureDailyJournal.Services
{
    public class JournalService
    {
        private readonly DatabaseService _databaseService;

        public JournalService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<JournalEntry> GetEntryForDateAsync(DateTime date)
        {
            return await _databaseService.GetEntryByDateAsync(date);
        }

        public async Task<JournalEntry> CreateEntryForTodayAsync()
        {
            var today = DateTime.Today;
            var existing = await GetEntryForDateAsync(today);
            if (existing != null)
                return existing;

            var newEntry = new JournalEntry
            {
                EntryDate = today,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Title = $"Entry for {today:D}" // Default title
            };
            await _databaseService.SaveEntryAsync(newEntry);
            return newEntry;
        }

        public async Task SaveEntryAsync(JournalEntry entry)
        {
            entry.UpdatedAt = DateTime.Now;
            // Ensure date is just Date, though DatabaseService handles it too.
            entry.EntryDate = entry.EntryDate.Date;
            await _databaseService.SaveEntryAsync(entry);
        }

        public async Task DeleteEntryAsync(JournalEntry entry)
        {
            await _databaseService.DeleteEntryAsync(entry);
        }

        public async Task<List<JournalEntry>> GetEntriesForMonthAsync(DateTime month)
        {
            await _databaseService.Init(); // Ensure Init
            // This is efficient enough for sqlite local
            var start = new DateTime(month.Year, month.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            
            // In SQL: WHERE EntryDate >= start AND EntryDate <= end
            // Since EntryDate is DateTime in C#, we need to be careful with comparison if ticks are used.
            // But usually this works:
            var all = await _databaseService.GetEntriesAsync(); // Or add filter in DB service
            // Better to filter in memory if list is small, or add specific query in DB service. 
            // Given "Individual Project" size, memory filter is fine for < 3650 entries (10 years).
            
            return all.Where(e => e.EntryDate.Year == month.Year && e.EntryDate.Month == month.Month).ToList();
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            return await _databaseService.GetEntriesAsync();
        }

        // Logic to check if entry exists for today
        public async Task<bool> HasEntryForDateAsync(DateTime date)
        {
            var entry = await GetEntryForDateAsync(date);
            return entry != null;
        }
        public async Task SaveTagsForEntryAsync(int entryId, List<string> tags)
        {
            await _databaseService.SaveTagsForEntryAsync(entryId, tags);
        }
        
    }
}
