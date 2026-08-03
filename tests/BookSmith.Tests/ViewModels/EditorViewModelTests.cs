using Xunit;
using BookSmith.UI.ViewModels.Main;

namespace BookSmith.Tests.ViewModels;

public class EditorViewModelTests
{
    private EditorViewModel CreateViewModel()
    {
        return new EditorViewModel(epubExporter: null);
    }

    #region Live Counters

    [Fact]
    public void WordCount_Updates_When_CleanedText_Changes()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Hello world foo bar";
        Assert.Equal(4, vm.WordCount);
    }

    [Fact]
    public void LineCount_Updates_When_CleanedText_Changes()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Line 1\nLine 2\nLine 3";
        Assert.Equal(3, vm.LineCount);
    }

    [Fact]
    public void Counters_Are_Zero_When_Text_Is_Empty()
    {
        var vm = CreateViewModel();
        vm.CleanedText = "";
        Assert.Equal(0, vm.WordCount);
        Assert.Equal(0, vm.LineCount);
    }

    #endregion

    #region Search & Replace

    [Fact]
    public void SearchResultCount_Updates_On_SearchQuery_Change()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Hello world. Hello again. Hello!";
        vm.IsSearchPanelVisible = true;
        vm.SearchQuery = "Hello";
        Assert.Equal(3, vm.SearchResultCount);
    }

    [Fact]
    public void SearchResultCount_Is_CaseInsensitive()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Hello HELLO hello";
        vm.IsSearchPanelVisible = true;
        vm.SearchQuery = "hello";
        Assert.Equal(3, vm.SearchResultCount);
    }

    [Fact]
    public void SearchResultCount_Is_Zero_When_No_Match()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Hello world";
        vm.IsSearchPanelVisible = true;
        vm.SearchQuery = "xyz";
        Assert.Equal(0, vm.SearchResultCount);
    }

    [Fact]
    public void ReplaceCurrent_Replaces_First_Occurrence()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "foo bar foo baz";
        vm.IsSearchPanelVisible = true;
        vm.SearchQuery = "foo";
        vm.ReplaceQuery = "qux";
        vm.OnReplaceCurrent();
        Assert.Equal("qux bar foo baz", vm.CleanedText);
    }

    [Fact]
    public void ReplaceAll_Replaces_All_Occurrences()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "foo bar foo baz foo";
        vm.IsSearchPanelVisible = true;
        vm.SearchQuery = "foo";
        vm.ReplaceQuery = "qux";
        vm.OnReplaceAll();
        Assert.Equal("qux bar qux baz qux", vm.CleanedText);
    }

    [Fact]
    public void ReplaceAll_Is_CaseInsensitive()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Foo FOO foo";
        vm.IsSearchPanelVisible = true;
        vm.SearchQuery = "foo";
        vm.ReplaceQuery = "bar";
        vm.OnReplaceAll();
        Assert.Equal("bar bar bar", vm.CleanedText);
    }

    [Fact]
    public void ToggleSearch_Opens_And_Closes_Panel()
    {
        var vm = CreateViewModel();
        Assert.False(vm.IsSearchPanelVisible);

        vm.ToggleSearchCommand.Execute(null);
        Assert.True(vm.IsSearchPanelVisible);

        vm.ToggleSearchCommand.Execute(null);
        Assert.False(vm.IsSearchPanelVisible);
    }

    [Fact]
    public void ToggleSearch_Close_Clears_Query()
    {
        var vm = CreateViewModel();
        vm.IsSearchPanelVisible = true;
        vm.SearchQuery = "test";
        vm.ReplaceQuery = "replace";

        vm.ToggleSearchCommand.Execute(null);
        Assert.Equal(string.Empty, vm.SearchQuery);
        Assert.Equal(string.Empty, vm.ReplaceQuery);
        Assert.Equal(0, vm.SearchResultCount);
    }

    #endregion

    #region Undo / Redo

    [Fact]
    public void Undo_Restores_Previous_Text()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "original text";
        vm.CleanedText = "modified text";

        vm.OnUndo();
        Assert.Equal("original text", vm.CleanedText);
    }

    [Fact]
    public void Redo_Restores_Undone_Text()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "original text";
        vm.CleanedText = "modified text";

        vm.OnUndo();
        Assert.Equal("original text", vm.CleanedText);

        vm.OnRedo();
        Assert.Equal("modified text", vm.CleanedText);
    }

    [Fact]
    public void Undo_Does_Nothing_When_Stack_Empty()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "only text";

        vm.OnUndo(); // should not crash
        Assert.Equal("only text", vm.CleanedText);
    }

    [Fact]
    public void Redo_Does_Nothing_When_Stack_Empty()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "only text";

        vm.OnRedo(); // should not crash
        Assert.Equal("only text", vm.CleanedText);
    }

    [Fact]
    public void Multiple_Undos_Walk_Back_History()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "state 1";
        vm.CleanedText = "state 2";
        vm.CleanedText = "state 3";

        vm.OnUndo();
        Assert.Equal("state 2", vm.CleanedText);

        vm.OnUndo();
        Assert.Equal("state 1", vm.CleanedText);
    }

    [Fact]
    public void New_Edit_After_Undo_Clears_Redo_Stack()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "state 1";
        vm.CleanedText = "state 2";

        vm.OnUndo(); // back to "state 1"
        vm.CleanedText = "state 3"; // new edit

        vm.OnRedo(); // redo stack should be empty
        Assert.Equal("state 3", vm.CleanedText);
    }

    #endregion

    #region Remove Selected Lines

    [Fact]
    public void RemoveSelectedLines_Removes_Selected_Text()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Line 1\nLine 2\nLine 3";
        vm.SelectedText = "Line 2";
        vm.OnRemoveSelectedLines();
        Assert.Equal("Line 1\nLine 3", vm.CleanedText);
    }

    [Fact]
    public void RemoveSelectedLines_Does_Nothing_When_No_Selection()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Line 1\nLine 2";
        vm.SelectedText = "";
        vm.OnRemoveSelectedLines();
        Assert.Equal("Line 1\nLine 2", vm.CleanedText);
    }

    [Fact]
    public void RemoveSelectedLines_Does_Nothing_When_Not_Found()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Line 1\nLine 2";
        vm.SelectedText = "Line 99";
        vm.OnRemoveSelectedLines();
        Assert.Equal("Line 1\nLine 2", vm.CleanedText);
    }

    #endregion

    #region Statistics

    [Fact]
    public void CharCounts_Update_On_CleanedText_Change()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 100;
        vm.CleanedText = "Hello";
        Assert.Equal(5, vm.CleanedCharCount);
        Assert.Equal(95, vm.RemovedCharCount);
    }

    [Fact]
    public void ReductionPercentage_Calculates_Correctly()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 200;
        vm.CleanedText = new string('a', 150);
        Assert.Equal(25.0, vm.ReductionPercentage);
    }

    [Fact]
    public void ReductionPercentage_Is_Zero_When_No_Original()
    {
        var vm = CreateViewModel();
        vm.OriginalCharCount = 0;
        vm.CleanedText = "";
        Assert.Equal(0, vm.ReductionPercentage);
    }

    #endregion
}
