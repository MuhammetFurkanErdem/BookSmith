namespace BookSmith.Core.Models;

public class AppSettings
{
    public bool RemoveHeaders { get; set; } = true;
    public bool RemoveFooters { get; set; } = true;
    public bool RemovePageNumbers { get; set; } = true;
    public bool FixBrokenWords { get; set; } = true;
    public bool MergeWrappedLines { get; set; } = true;
    public bool SmartDialogueFormatting { get; set; } = true;
    public bool ElevenReaderMode { get; set; } = false;
    public bool ExportEpub { get; set; } = false;
    public string DefaultLanguage { get; set; } = "tr";
}
