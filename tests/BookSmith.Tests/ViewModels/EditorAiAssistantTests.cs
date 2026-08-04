using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.ViewModels.Main;
using Xunit;

namespace BookSmith.Tests.ViewModels;

public class EditorAiAssistantTests
{
    private class DummyAiService : IAiReconstructionService
    {
        public Task<string> ReconstructParagraphAsync(string text, AiModelConfig config, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("Geralt diş izleri vardı. Sonra her şey çok hızlı gelişti.");
        }

        public Task<bool> TestConnectionAsync(AiModelConfig config, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    private EditorViewModel CreateViewModel()
    {
        var dummyAi = new DummyAiService();
        return new EditorViewModel(null, null, dummyAi);
    }

    [Fact]
    public void EditorViewModel_ToggleAiPanel_UpdatesVisibility()
    {
        var vm = CreateViewModel();

        Assert.False(vm.IsAiPanelVisible);

        vm.ToggleAiPanelCommand.Execute(null);

        Assert.True(vm.IsAiPanelVisible);
    }

    [Fact]
    public void EditorViewModel_AcceptAiFix_ReplacesTargetTextInDocument()
    {
        var vm = CreateViewModel();
        vm.CleanedText = "Geralt diş izleri vrdı. ANDRZEJ SAPKOWSKI Sonra her şey çok hızlı gelişti.";
        vm.OriginalDiffText = "Geralt diş izleri vrdı. ANDRZEJ SAPKOWSKI Sonra her şey çok hızlı gelişti.";
        vm.AiRestoredDiffText = "Geralt diş izleri vardı. Sonra her şey çok hızlı gelişti.";
        vm.IsDiffModalVisible = true;

        vm.AcceptAiFixCommand.Execute(null);

        Assert.Equal("Geralt diş izleri vardı. Sonra her şey çok hızlı gelişti.", vm.CleanedText);
        Assert.False(vm.IsDiffModalVisible);
    }

    [Fact]
    public void EditorViewModel_RejectAiFix_ClosesModalWithoutModifyingText()
    {
        var vm = CreateViewModel();
        string original = "Geralt diş izleri vrdı. ANDRZEJ SAPKOWSKI Sonra her şey çok hızlı gelişti.";
        vm.CleanedText = original;
        vm.IsDiffModalVisible = true;

        vm.RejectAiFixCommand.Execute(null);

        Assert.Equal(original, vm.CleanedText);
        Assert.False(vm.IsDiffModalVisible);
    }
}
