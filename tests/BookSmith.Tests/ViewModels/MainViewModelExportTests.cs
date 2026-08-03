using System;
using System.Collections.Generic;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.ViewModels.Main;
using Xunit;

namespace BookSmith.Tests.ViewModels;

public class MainViewModelExportTests
{
    private class DummyPdfReader : IPdfReader
    {
        public PdfMetadata ReadMetadata(string filePath) => new PdfMetadata();
        public string ReadFirstPageText(string filePath) => string.Empty;
        public IReadOnlyList<string> ReadAllPages(string filePath) => new List<string>();
    }

    private class DummyTextCleaner : ITextCleaner
    {
        public TextCleaningResult Clean(string input) => new TextCleaningResult();
        public string RemoveHeadersAndFooters(IReadOnlyList<string> pages) => string.Empty;
        public string FormatDialogueForTts(string text) => text;
    }

    private class DummyBookPipeline : IBookPipeline
    {
        public BookProcessingResult Process(string filePath) => new BookProcessingResult();
    }

    private class MockEpubExporter : IEpubExporter
    {
        public bool ExportCalled { get; private set; }
        public EpubExportOptions? Options { get; private set; }

        public void Export(EpubExportOptions options)
        {
            ExportCalled = true;
            Options = options;
        }
    }

    [Fact]
    public void ExportCommands_WhenNotCleaned_CannotExecute()
    {
        var vm = new MainViewModel(new DummyPdfReader(), new DummyTextCleaner(), new DummyBookPipeline(), null, new MockEpubExporter());

        Assert.False(vm.ExportEpubCommand.CanExecute(null));
        Assert.False(vm.ExportTxtCommand.CanExecute(null));
    }

    [Fact]
    public void ExportCommands_WhenCleanedAndTextPresent_CanExecute()
    {
        var vm = new MainViewModel(new DummyPdfReader(), new DummyTextCleaner(), new DummyBookPipeline(), null, new MockEpubExporter())
        {
            IsCleaned = true,
            CleanedText = "Sample cleaned book content"
        };

        Assert.True(vm.ExportEpubCommand.CanExecute(null));
        Assert.True(vm.ExportTxtCommand.CanExecute(null));
    }
}
