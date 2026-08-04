using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Pipeline;

public class BatchProcessor : IBatchProcessor
{
    private readonly IBookPipeline _bookPipeline;

    public BatchProcessor(IBookPipeline bookPipeline)
    {
        _bookPipeline = bookPipeline ?? throw new ArgumentNullException(nameof(bookPipeline));
    }

    public async Task<IReadOnlyList<BatchItem>> ProcessBatchAsync(
        IReadOnlyList<string> filePaths,
        CleaningPreset preset,
        Action<int, int, string>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (filePaths == null || filePaths.Count == 0)
            return Array.Empty<BatchItem>();

        var items = new List<BatchItem>();
        foreach (string path in filePaths)
        {
            items.Add(new BatchItem
            {
                FilePath = path,
                Status = BatchStatus.Pending,
                StatusMessage = "Queued"
            });
        }

        int total = items.Count;

        for (int i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var item = items[i];
            item.Status = BatchStatus.Processing;
            item.StatusMessage = "Processing...";
            progressCallback?.Invoke(i + 1, total, $"Processing {item.FileName} ({i + 1}/{total})...");

            try
            {
                string path = item.FilePath;
                bool removeFrontMatter = preset?.RemoveFrontMatter ?? true;

                var result = await Task.Run(() => _bookPipeline.Process(path, removeFrontMatter), cancellationToken);

                item.Result = result;
                item.Status = BatchStatus.Completed;
                item.StatusMessage = $"Completed ({result.CleanedCharCount:N0} chars)";
            }
            catch (Exception ex)
            {
                item.Status = BatchStatus.Failed;
                item.StatusMessage = $"Failed: {ex.Message}";
            }

            progressCallback?.Invoke(i + 1, total, $"Completed {item.FileName} ({i + 1}/{total})");
        }

        return items.AsReadOnly();
    }
}
