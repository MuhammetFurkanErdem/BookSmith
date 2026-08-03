namespace BookSmith.Core.Models;

public class EpubExportOptions
{
    public string Title { get; set; } = "Untitled Book";
    public string Author { get; set; } = "Unknown Author";
    public string Language { get; set; } = "tr";
    public string OutputPath { get; set; } = string.Empty;
    public string ContentText { get; set; } = string.Empty;
}
