using System.IO;

namespace BookSmith.Core.Models;

public enum BatchStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

/// <summary>
/// Represents an individual PDF file in a batch processing queue.
/// </summary>
public class BatchItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FilePath);
    public BatchStatus Status { get; set; } = BatchStatus.Pending;
    public string StatusMessage { get; set; } = "Queued";
    public BookProcessingResult? Result { get; set; }
}
