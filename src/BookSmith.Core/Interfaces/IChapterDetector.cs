using System.Collections.Generic;
using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

/// <summary>
/// Detects chapter headings within cleaned book text and returns their positions
/// so the editor can display a navigable table of contents.
/// </summary>
public interface IChapterDetector
{
    /// <summary>
    /// Scans <paramref name="cleanedText"/> for chapter headings and returns
    /// an ordered list of detected chapters with their character offsets.
    /// </summary>
    IReadOnlyList<ChapterInfo> DetectChapters(string cleanedText);
}
