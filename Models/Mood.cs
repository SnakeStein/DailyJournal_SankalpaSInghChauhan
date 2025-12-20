using SQLite;

namespace SecureDailyJournal.Models
{
    public class Mood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = "Neutral"; // Positive, Neutral, Negative
        public string Icon { get; set; } = string.Empty; // Emoji or icon name
    }
}
