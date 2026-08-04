using System.Collections.Generic;

namespace BookSmith.Core.Interfaces;

/// <summary>
/// Detects and strips publisher front-matter pages (copyright, ISBN, translator credits, etc.)
/// from the beginning of a book before the main content cleaning pipeline runs.
/// </summary>
public interface IFrontMatterFilter
{
    /// <summary>
    /// Scans the first pages for front-matter content, removes them, and returns
    /// the remaining pages as a list.
    /// </summary>
    IReadOnlyList<string> FilterFrontMatterPages(IReadOnlyList<string> pages);

    /// <summary>
    /// Scans the first pages for front-matter content, removes them, and returns
    /// the remaining text joined ready for the next pipeline stage.
    /// </summary>
    /// <param name="pages">All pages extracted from the PDF.</param>
    /// <returns>Text content with front-matter pages stripped.</returns>
    string FilterFrontMatter(IReadOnlyList<string> pages);
}
