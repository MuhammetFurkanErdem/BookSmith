using System;
using System.Collections.Generic;
using System.IO;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.Services.Cleaning;
using BookSmith.Services.Pipeline;
using Xunit;

namespace BookSmith.Tests.Pipeline;

public class BookPipelineTests
{
    private class FakePdfReader : IPdfReader
    {
        private readonly IReadOnlyList<string> _pages;

        public FakePdfReader(IReadOnlyList<string> pages)
        {
            _pages = pages;
        }

        public PdfMetadata ReadMetadata(string filePath) => new PdfMetadata { FileName = Path.GetFileName(filePath), PageCount = _pages.Count, FileSize = 100 };
        public string ReadFirstPageText(string filePath) => _pages.Count > 0 ? _pages[0] : string.Empty;
        public IReadOnlyList<string> ReadAllPages(string filePath) => _pages;
    }

    [Fact]
    public void Constructor_WithNullPdfReader_ThrowsArgumentNullException()
    {
        var textCleaner = new TextCleaner();
        Assert.Throws<ArgumentNullException>(() => new BookPipeline(null!, textCleaner));
    }

    [Fact]
    public void Constructor_WithNullTextCleaner_ThrowsArgumentNullException()
    {
        var pdfReader = new FakePdfReader(new List<string>());
        Assert.Throws<ArgumentNullException>(() => new BookPipeline(pdfReader, null!));
    }

    [Fact]
    public void Process_WithNullFilePath_ThrowsArgumentException()
    {
        var pipeline = new BookPipeline(new FakePdfReader(new List<string>()), new TextCleaner());
        Assert.Throws<ArgumentException>(() => pipeline.Process(null!));
    }

    [Fact]
    public void Process_WithEmptyFilePath_ThrowsArgumentException()
    {
        var pipeline = new BookPipeline(new FakePdfReader(new List<string>()), new TextCleaner());
        Assert.Throws<ArgumentException>(() => pipeline.Process(string.Empty));
    }

    [Fact]
    public void Process_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var pipeline = new BookPipeline(new FakePdfReader(new List<string>()), new TextCleaner());
        string nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
        Assert.Throws<FileNotFoundException>(() => pipeline.Process(nonExistent));
    }

    [Fact]
    public void Process_WithValidFile_ExecutesPipelineAndReturnsResult()
    {
        // Arrange
        string tempFile = Path.GetTempFileName();
        try
        {
            var pages = new List<string>
            {
                "Book Smith Title\n\nChapter 1\nThis is a sample line that is wrapped\nand should be merged.",
                "Book Smith Title\n\nChapter 2\nThis is another sample line.\nSoft hyphen test " + '\u00AD' + " here.",
                "Book Smith Title\n\nChapter 3\nFinal line of the book."
            };

            var fakePdfReader = new FakePdfReader(pages);
            var textCleaner = new TextCleaner();
            var pipeline = new BookPipeline(fakePdfReader, textCleaner);

            // Act
            BookProcessingResult result = pipeline.Process(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tempFile, result.FilePath);
            Assert.Equal(3, result.TotalPages);
            Assert.Contains("Chapter 1", result.CleanedText);
            Assert.Contains("Chapter 2", result.CleanedText);
            Assert.Contains("Chapter 3", result.CleanedText);
            Assert.DoesNotContain("Book Smith Title", result.CleanedText);
            Assert.False(result.CleanedText.Contains('\u00AD'));
            Assert.True(result.CleanedCharCount < result.OriginalCharCount);
            Assert.True(result.ReductionPercentage > 0);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
