using System;
using System.Collections.Generic;

namespace BookSmith.Core.Models;

/// <summary>
/// Represents a detected misspelled or corrupted word with character offset and correction suggestions.
/// </summary>
public class MisspelledWord
{
    public string Word { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int Length => Word.Length;
    public IReadOnlyList<string> Suggestions { get; set; } = Array.Empty<string>();
}
