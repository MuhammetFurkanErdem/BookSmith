namespace BookSmith.Core.Models;

/// <summary>Represents a detected chapter heading within cleaned book text.</summary>
public class ChapterInfo
{
    /// <summary>1-based chapter index in detection order.</summary>
    public int Index { get; set; }

    /// <summary>The detected heading text, e.g. "BÖLÜM 1" or "Chapter IV".</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Character offset within the full cleaned text where this heading starts.</summary>
    public int CharOffset { get; set; }
}
