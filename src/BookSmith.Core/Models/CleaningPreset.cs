using System;

namespace BookSmith.Core.Models;

/// <summary>
/// Represents a named preset configuration of text cleaning rules.
/// </summary>
public class CleaningPreset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; } = false;

    public bool RemoveHeaders { get; set; } = true;
    public bool RemoveFooters { get; set; } = true;
    public bool RemovePageNumbers { get; set; } = true;
    public bool FixBrokenWords { get; set; } = true;
    public bool MergeWrappedLines { get; set; } = true;
    public bool SmartDialogueFormatting { get; set; } = true;
    public bool ElevenReaderMode { get; set; } = false;
    public bool ExportEpub { get; set; } = false;
    public bool RemoveFrontMatter { get; set; } = true;
}
