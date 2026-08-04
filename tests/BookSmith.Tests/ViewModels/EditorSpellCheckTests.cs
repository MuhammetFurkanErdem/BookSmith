using System;
using BookSmith.Services.Cleaning;
using BookSmith.UI.ViewModels.Main;
using Xunit;

namespace BookSmith.Tests.ViewModels;

public class EditorSpellCheckTests
{
    private EditorViewModel CreateViewModel()
    {
        var spellChecker = new TurkishSpellChecker();
        return new EditorViewModel(null, spellChecker);
    }

    [Fact]
    public void EditorViewModel_ToggleSpellCheck_UpdatesStatusText()
    {
        var vm = CreateViewModel();

        Assert.True(vm.IsSpellCheckEnabled);
        Assert.Contains("ON", vm.SpellCheckStatusText);

        vm.ToggleSpellCheckCommand.Execute(null);

        Assert.False(vm.IsSpellCheckEnabled);
        Assert.Contains("OFF", vm.SpellCheckStatusText);
    }

    [Fact]
    public void EditorViewModel_ApplySuggestion_ReplacesTargetWord()
    {
        var vm = CreateViewModel();
        vm.CleanedText = "Geralt buyucu ile konuştu.";

        var tuple = new Tuple<string, string>("buyucu", "büyücü");
        vm.ApplySuggestionCommand.Execute(tuple);

        Assert.Equal("Geralt büyücü ile konuştu.", vm.CleanedText);
    }
}
