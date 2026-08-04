using System.Collections.Generic;
using System.Linq;

namespace BookSmith.Core.Models;

public class EpubExportOptions
{
    public string Title { get; set; } = "Untitled Book";
    public string Author { get; set; } = "Unknown Author";
    public string Language { get; set; } = "tr";
    public string OutputPath { get; set; } = string.Empty;
    public string ContentText { get; set; } = string.Empty;
    public IReadOnlyList<ChapterInfo> Chapters { get; set; } = Array.Empty<ChapterInfo>();
    public string? CoverImagePath { get; set; }
    public int FontSizePt { get; set; } = 12;
    public double LineHeight { get; set; } = 1.6;
}
