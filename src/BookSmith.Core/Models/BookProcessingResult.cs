using System;
using System.Collections.Generic;

namespace BookSmith.Core.Models;

public class BookProcessingResult
{
    public string FilePath { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public string CleanedText { get; set; } = string.Empty;
    public IReadOnlyList<ChapterInfo> Chapters { get; set; } = Array.Empty<ChapterInfo>();

    public int OriginalCharCount => OriginalText.Length;
    public int CleanedCharCount => CleanedText.Length;
    public int RemovedCharCount => Math.Max(0, OriginalCharCount - CleanedCharCount);

    public double ReductionPercentage => OriginalCharCount == 0 
        ? 0 
        : Math.Round((double)RemovedCharCount / OriginalCharCount * 100.0, 2);
}
