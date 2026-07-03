namespace BookSmith.Core.Models;

public class TextCleaningResult
{
    public string CleanedText { get; set; } = string.Empty;
    public bool IsModified { get; set; }
}
