using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

/// <summary>
/// Service for sequentially processing multiple PDF book files in a batch queue.
/// </summary>
public interface IBatchProcessor
{
    /// <summary>
    /// Sequentially processes all <paramref name="filePaths"/> using the given <paramref name="preset"/>.
    /// Reports progress via optional callback.
    /// </summary>
    Task<IReadOnlyList<BatchItem>> ProcessBatchAsync(
        IReadOnlyList<string> filePaths,
        CleaningPreset preset,
        Action<int, int, string>? progressCallback = null,
        CancellationToken cancellationToken = default);
}
