using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.Services.Pipeline;
using Xunit;

namespace BookSmith.Tests.Services;

public class BatchProcessorTests
{
    private class DummyPipeline : IBookPipeline
    {
        public BookProcessingResult Process(string filePath, bool removeFrontMatter = true)
        {
            if (filePath.Contains("invalid"))
                throw new InvalidOperationException("Failed to read file.");

            return new BookProcessingResult
            {
                FilePath = filePath,
                TotalPages = 10,
                OriginalText = "Original content",
                CleanedText = "Cleaned text for " + filePath
            };
        }
    }

    [Fact]
    public async Task ProcessBatchAsync_ProcessesAllFilesSequentially()
    {
        var pipeline = new DummyPipeline();
        var processor = new BatchProcessor(pipeline);

        var files = new List<string> { "file1.pdf", "file2.pdf", "file3.pdf" };
        var preset = new CleaningPreset { RemoveFrontMatter = true };

        int progressCalls = 0;
        var results = await processor.ProcessBatchAsync(files, preset, (current, total, msg) =>
        {
            progressCalls++;
        });

        Assert.Equal(3, results.Count);
        Assert.All(results, item => Assert.Equal(BatchStatus.Completed, item.Status));
        Assert.True(progressCalls > 0);
    }

    [Fact]
    public async Task ProcessBatchAsync_HandlesFileFailureGracefully()
    {
        var pipeline = new DummyPipeline();
        var processor = new BatchProcessor(pipeline);

        var files = new List<string> { "good.pdf", "invalid.pdf", "another_good.pdf" };
        var preset = new CleaningPreset();

        var results = await processor.ProcessBatchAsync(files, preset);

        Assert.Equal(3, results.Count);
        Assert.Equal(BatchStatus.Completed, results[0].Status);
        Assert.Equal(BatchStatus.Failed, results[1].Status);
        Assert.Equal(BatchStatus.Completed, results[2].Status);
    }
}
