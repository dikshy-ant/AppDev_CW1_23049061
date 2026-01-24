using Microsoft.EntityFrameworkCore;
using MyJournal.Data;
using MyJournal.Entities;
using MyJournal.Models.Display;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Text;
using Colors = QuestPDF.Helpers.Colors;

namespace MyJournal.Services
{
    public class ExportService
    {
        private readonly AppDbContext _context;

        public ExportService(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> ExportToPdfAsync(DateTime startDate, DateTime endDate)
        {
            // Get entries for the date range
            var entries = await _context.JournalEntries
                .Include(je => je.PrimaryMood)
                .Include(je => je.Category)
                .Include(je => je.SecondaryMoods).ThenInclude(jem => jem.Mood)
                .Include(je => je.Tags).ThenInclude(jet => jet.Tag)
                .Where(je => je.Date.Date >= startDate.Date && je.Date.Date <= endDate.Date)
                .OrderByDescending(je => je.Date)
                .ToListAsync();

            return GeneratePdfDocument(entries, startDate, endDate);
        }

        private byte[] GeneratePdfDocument(List<JournalEntry> entries, DateTime startDate, DateTime endDate)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri));

                    // Header
                    page.Header().Column(header =>
                    {
                        header.Item().Text("My Journal Export")
                            .FontSize(20)
                            .FontColor(Colors.Blue.Medium)
                            .Bold()
                            .AlignCenter();

                        header.Item().Text($"{startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken2)
                            .AlignCenter();
                    });

                    // Content
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        if (!entries.Any())
                        {
                            column.Item().Text("No journal entries found for the selected date range.")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1)
                                .AlignCenter();
                            return;
                        }

                        foreach (var entry in entries)
                        {
                            // Entry Header
                            column.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Column(entryHeader =>
                                {
                                    entryHeader.Item().Text(entry.Date.ToString("MMMM dd, yyyy"))
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken1);

                                    if (entry.PrimaryMood != null)
                                    {
                                        entryHeader.Item().Text($"Mood: {entry.PrimaryMood.Icon} {entry.PrimaryMood.Name}")
                                            .FontSize(11)
                                            .FontColor(Colors.Grey.Darken2);
                                    }

                                    if (entry.Category != null)
                                    {
                                        entryHeader.Item().Text($"Category: {entry.Category.Name}")
                                            .FontSize(11)
                                            .FontColor(Colors.Grey.Darken2);
                                    }
                                });

                                row.ConstantItem(80).Text($"{StripHtmlTags(entry.Content).Split(' ', StringSplitOptions.RemoveEmptyEntries).Length} words")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken2)
                                    .AlignRight();
                            });

                            // Entry Content
                            var cleanContent = StripHtmlTags(entry.Content);
                            if (!string.IsNullOrEmpty(cleanContent))
                            {
                                column.Item().PaddingBottom(5).Text(cleanContent)
                                    .FontSize(11)
                                    .LineHeight(1.4f);
                            }

                            // Tags
                            if (entry.Tags.Any())
                            {
                                column.Item().PaddingBottom(5).Text($"Tags: {string.Join(", ", entry.Tags.Select(t => "#" + t.Tag.Name))}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken2)
                                    .Italic();
                            }

                            // Separator
                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        }
                    });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text("My Journal Export")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken2);
                });
            })
            .GeneratePdf();
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Replace paragraph tags with line breaks
            var processed = html.Replace("</p>", "\n\n").Replace("<p>", "");
            
            // Replace line break tags with line breaks
            processed = processed.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");
            
            // Replace div tags with line breaks
            processed = processed.Replace("</div>", "\n").Replace("<div>", "");
            
            // Remove all other HTML tags
            processed = System.Text.RegularExpressions.Regex.Replace(processed, "<[^>]*>", string.Empty);
            
            // Decode HTML entities
            processed = System.Web.HttpUtility.HtmlDecode(processed);
            
            // Clean up multiple consecutive line breaks
            processed = System.Text.RegularExpressions.Regex.Replace(processed, @"\n{3,}", "\n\n");
            
            // Clean up multiple whitespace but preserve line breaks
            var lines = processed.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = System.Text.RegularExpressions.Regex.Replace(lines[i], @"\s+", " ").Trim();
            }
            
            return string.Join('\n', lines).Trim();
        }
    }
}
