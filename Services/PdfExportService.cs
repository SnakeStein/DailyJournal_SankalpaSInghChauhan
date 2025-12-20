using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SecureDailyJournal.Models;

namespace SecureDailyJournal.Services
{
    public class PdfExportService
    {
        public PdfExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> ExportToPdfAsync(List<JournalEntry> entries, DateTime start, DateTime end)
        {
            var fileName = $"Journal_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName); // Default to cache, user can save elsewhere

            // If we want to prompt user for location, we can't easily do that in cross-platform service logic without UI.
            // On Windows (Desktop), we can save to MyDocuments/JournalExports.
            
            var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var exportFolder = Path.Combine(docsPath, "SecureDailyJournal Exports");
            Directory.CreateDirectory(exportFolder);
            filePath = Path.Combine(exportFolder, fileName);
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    
                    page.Header()
                        .Text($"Journal Entries: {start:D} - {end:D}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            foreach (var entry in entries)
                            {
                                x.Item().ShowEntire().Column(item => 
                                {
                                    item.Item().Text($"{entry.EntryDate:D} - {entry.Title}").Bold().FontSize(14);
                                    item.Item().Text($"Mood: {entry.PrimaryMood}").Italic().FontSize(10);
                                    item.Item().PaddingTop(5).Text(entry.Content).FontSize(12);
                                    item.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf(filePath);

            return filePath;
        }
    }
}
