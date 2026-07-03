namespace BookSmith.Core.Models;

public class PdfMetadata
{
    public string FileName { get; set; } = string.Empty;
    public double FileSize { get; set; } // in KB
    public int PageCount { get; set; }
}
