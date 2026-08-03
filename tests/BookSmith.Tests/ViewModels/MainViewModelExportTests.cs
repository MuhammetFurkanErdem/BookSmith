using System;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.ViewModels.Main;
using Xunit;

namespace BookSmith.Tests.ViewModels;

public class MainViewModelExportTests
{
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
    public void ExportCommands_WhenTextIsEmpty_CannotExecute()
    {
        var editorVm = new EditorViewModel(new MockEpubExporter())
        {
            CleanedText = string.Empty
        };

        Assert.False(editorVm.ExportEpubCommand.CanExecute(null));
        Assert.False(editorVm.ExportTxtCommand.CanExecute(null));
    }

    [Fact]
    public void ExportCommands_WhenTextIsPresent_CanExecute()
    {
        var editorVm = new EditorViewModel(new MockEpubExporter())
        {
            CleanedText = "Sample cleaned book content"
        };

        Assert.True(editorVm.ExportEpubCommand.CanExecute(null));
        Assert.True(editorVm.ExportTxtCommand.CanExecute(null));
    }
}
