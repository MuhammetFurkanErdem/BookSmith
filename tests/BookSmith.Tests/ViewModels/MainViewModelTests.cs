using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.ViewModels.Main;
using Xunit;

namespace BookSmith.Tests.ViewModels;

public class MainViewModelTests
{
    private class MockPdfReader : IPdfReader
    {
        public PdfMetadata ReadMetadata(string filePath) => new PdfMetadata { FileName = "test.pdf", FileSize = 100, PageCount = 2 };
        public string ReadFirstPageText(string filePath) => "First page text";
        public IReadOnlyList<string> ReadAllPages(string filePath) => new List<string> { "Page 1 text", "Page 2 text" };
    }

    private class MockTextCleaner : ITextCleaner
    {
        public TextCleaningResult Clean(string input) => new TextCleaningResult { OriginalText = input, CleanedText = "Cleaned: " + input };
        public string RemoveHeadersAndFooters(IReadOnlyList<string> pages) => string.Join("\n\n", pages);
    }

    private class MockBookPipeline : IBookPipeline
    {
        public BookProcessingResult Process(string filePath)
        {
            return new BookProcessingResult
            {
                FilePath = filePath,
                TotalPages = 2,
                OriginalText = "Page 1 text\n\nPage 2 text",
                CleanedText = "Cleaned output text"
            };
        }
    }

    [Fact]
    public void Constructor_WithNullPdfReader_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(null!, new MockTextCleaner(), new MockBookPipeline()));
    }

    [Fact]
    public void Constructor_WithNullTextCleaner_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(new MockPdfReader(), null!, new MockBookPipeline()));
    }

    [Fact]
    public void Constructor_WithNullBookPipeline_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(new MockPdfReader(), new MockTextCleaner(), null!));
    }

    [Fact]
    public void CanStartCleaning_WhenFilePathIsEmpty_ReturnsFalse()
    {
        var vm = new MainViewModel(new MockPdfReader(), new MockTextCleaner(), new MockBookPipeline());
        Assert.False(vm.StartCleaningCommand.CanExecute(null));
    }

    [Fact]
    public void CanStartCleaning_WhenFilePathIsSet_ReturnsTrue()
    {
        var vm = new MainViewModel(new MockPdfReader(), new MockTextCleaner(), new MockBookPipeline())
        {
            FilePath = "C:\\test.pdf"
        };
        Assert.True(vm.StartCleaningCommand.CanExecute(null));
    }

    [Fact]
    public async Task StartCleaningCommand_ExecutesPipelineAndUpdatesProperties()
    {
        var vm = new MainViewModel(new MockPdfReader(), new MockTextCleaner(), new MockBookPipeline())
        {
            FilePath = "C:\\test.pdf"
        };

        Assert.False(vm.IsCleaned);
        Assert.Equal(0, vm.ProgressValue);

        if (vm.StartCleaningCommand.CanExecute(null))
        {
            vm.StartCleaningCommand.Execute(null);
            // Wait brief moment for AsyncRelayCommand task
            await Task.Delay(100);
        }

        Assert.True(vm.IsCleaned);
        Assert.Equal(100, vm.ProgressValue);
        Assert.Equal("Cleaned output text", vm.CleanedText);
        Assert.Contains("Completed!", vm.StatusText);
    }
}
