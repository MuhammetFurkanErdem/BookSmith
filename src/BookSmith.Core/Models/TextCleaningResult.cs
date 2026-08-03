namespace BookSmith.Core.Models;

public class TextCleaningResult
{
    public string OriginalText { get; set; } = string.Empty;
    public string CleanedText { get; set; } = string.Empty;
    public bool IsModified => OriginalText != CleanedText;
}
