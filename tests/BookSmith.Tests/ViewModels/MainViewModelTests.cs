using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.Navigation;
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
        public string FormatDialogueForTts(string text) => text;
    }

    private class MockBookPipeline : IBookPipeline
    {
        public BookProcessingResult Process(string filePath, bool removeFrontMatter = true)
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
    public void Constructor_WithNullNavigationService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(null!, new MockPdfReader(), new MockTextCleaner(), new MockBookPipeline()));
    }

    [Fact]
    public void InitialState_NavigatesToImportViewModel()
    {
        var navService = new NavigationService();
        var vm = new MainViewModel(navService, new MockPdfReader(), new MockTextCleaner(), new MockBookPipeline());

        Assert.NotNull(navService.CurrentViewModel);
        Assert.IsType<ImportViewModel>(navService.CurrentViewModel);
        Assert.Equal(1, navService.CurrentStep);
    }
}
