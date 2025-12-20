using SQLite;

namespace SecureDailyJournal.Models
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public DateTime EntryDate { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PrimaryMood { get; set; } = string.Empty;
        
        // Secondary moods can be stored as comma separated string or ignored if using join table, 
        // but requirement says "Up to two secondary moods". 
        // Simplest is to store them here or use a relationship. 
        // Given complexity, storing as JSON or string might be easier, but let's stick to core or relationship if needed.
        // For now, I'll add fields for simplicity or better yet, just keep PrimaryMood as required and maybe handle secondary via tags or separate column.
        // Actually, "Mood Tracking" requirements say "One primary mood (required), Up to two secondary moods".
        public string SecondaryMoods { get; set; } = string.Empty; // Comma separated

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
