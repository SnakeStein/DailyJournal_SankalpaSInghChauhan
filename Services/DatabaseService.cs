using SQLite;
using SecureDailyJournal.Models;

namespace SecureDailyJournal.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
        }

        public async Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            
            // Create tables
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<JournalEntry>();
            await _database.CreateTableAsync<Mood>();
            await _database.CreateTableAsync<Tag>();
            await _database.CreateTableAsync<EntryTag>();
            
            // Seed initial data if needed (e.g. Moods)
            if (await _database.Table<Mood>().CountAsync() == 0)
            {
                await _database.InsertAllAsync(new List<Mood>
                {
                    new Mood { Name = "Happy", Category = "Positive", Icon = "😊" },
                    new Mood { Name = "Excited", Category = "Positive", Icon = "🤩" },
                    new Mood { Name = "Neutral", Category = "Neutral", Icon = "😐" },
                    new Mood { Name = "Sad", Category = "Negative", Icon = "😢" },
                    new Mood { Name = "Angry", Category = "Negative", Icon = "😠" }
                });
            }
        }

        public async Task<List<JournalEntry>> GetEntriesAsync()
        {
            await Init();
            return await _database.Table<JournalEntry>().OrderByDescending(e => e.EntryDate).ToListAsync();
        }

        public async Task<JournalEntry> GetEntryAsync(int id)
        {
            await Init();
            return await _database.Table<JournalEntry>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<JournalEntry> GetEntryByDateAsync(DateTime date)
        {
            await Init();
            // Compare dates only
            var dateOnly = date.Date;
            // SQLite stores DateTime as ticks or ISO string, equality check might be tricky if not careful.
            // But if we store date.Date, it should be fine.
            // Let's filter in memory or trust the query. 
            // Better: store Ticks or use a query that handles date ranges if needed. 
            // Actually, querying strictly by date in SQLite-net-pcl usually works if saved correctly.
            // Let's grab all and filter first to be safe or try direct query. 
            // Optimally: Where(e => e.EntryDate == dateOnly)
            return await _database.Table<JournalEntry>().Where(e => e.EntryDate == dateOnly).FirstOrDefaultAsync();
        }

        public async Task<int> SaveEntryAsync(JournalEntry entry)
        {
            await Init();
            if (entry.Id != 0)
            {
                entry.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(entry);
            }
            else
            {
                entry.EntryDate = entry.EntryDate.Date; // Ensure time is stripped
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                return await _database.InsertAsync(entry);
            }
        }

        public async Task<int> DeleteEntryAsync(JournalEntry entry)
        {
            await Init();
            return await _database.DeleteAsync(entry);
        }

        // User / Security
        public async Task<User> GetUserAsync()
        {
            await Init();
            return await _database.Table<User>().FirstOrDefaultAsync();
        }

        public async Task<int> SaveUserAsync(User user)
        {
            await Init();
            if (user.Id != 0)
                return await _database.UpdateAsync(user);
            else
                return await _database.InsertAsync(user);
        }
        
        // Moods
        public async Task<List<Mood>> GetMoodsAsync()
        {
            await Init();
            return await _database.Table<Mood>().ToListAsync();
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            await Init();
            return await _database.Table<Tag>().ToListAsync();
        }
        
       public async Task<List<Tag>> GetTagsForEntryAsync(int entryId)
        {
            await Init();
            // Manual join
            var entryTags = await _database.Table<EntryTag>().Where(et => et.EntryId == entryId).ToListAsync();
            var tagIds = entryTags.Select(et => et.TagId).ToList();
            
            // Not efficient for large sets in sqlite-net-pcl without raw query, but fine here
            var allTags = await _database.Table<Tag>().ToListAsync();
            return allTags.Where(t => tagIds.Contains(t.Id)).ToList();
        }

        public async Task SaveTagsForEntryAsync(int entryId, List<string> tagNames)
        {
            await Init();
            // Clear existing
            var existingMap = await _database.Table<EntryTag>().Where(et => et.EntryId == entryId).ToListAsync();
            foreach(var map in existingMap) await _database.DeleteAsync(map);

            foreach (var tagName in tagNames)
            {
                var tag = await _database.Table<Tag>().Where(t => t.Name == tagName).FirstOrDefaultAsync();
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    await _database.InsertAsync(tag);
                }
                
                await _database.InsertAsync(new EntryTag { EntryId = entryId, TagId = tag.Id });
            }
        }
    }
}
