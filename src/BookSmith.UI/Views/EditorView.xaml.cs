using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookSmith.UI.ViewModels.Main;

namespace BookSmith.UI.Views;

public partial class EditorView : UserControl
{
    private EditorViewModel? _currentVm;

    public EditorView()
    {
        InitializeComponent();
        PreviewKeyDown += EditorView_PreviewKeyDown;
        DataContextChanged += EditorView_DataContextChanged;
    }

    private void EditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Unsubscribe from old VM
        if (_currentVm != null)
        {
            _currentVm.FindNextRequested -= OnFindNextRequested;
            _currentVm.ScrollToChapterRequested -= OnScrollToChapterRequested;
        }

        // Subscribe to new VM
        _currentVm = DataContext as EditorViewModel;
        if (_currentVm != null)
        {
            _currentVm.FindNextRequested += OnFindNextRequested;
            _currentVm.ScrollToChapterRequested += OnScrollToChapterRequested;
        }
    }

    private void OnScrollToChapterRequested(int charOffset)
    {
        if (EditorTextBox == null || charOffset < 0 || charOffset >= EditorTextBox.Text.Length) return;

        EditorTextBox.Focus();
        EditorTextBox.Select(charOffset, 0);

        int lineIndex = EditorTextBox.GetLineIndexFromCharacterIndex(charOffset);
        if (lineIndex >= 0)
        {
            EditorTextBox.ScrollToLine(lineIndex);
        }
    }

    /// <summary>Called by ViewModel when Find Next locates a match — selects & scrolls to it in the TextBox.</summary>
    private void OnFindNextRequested(int startIndex, int length)
    {
        if (EditorTextBox == null) return;

        EditorTextBox.Focus();
        EditorTextBox.Select(startIndex, length);

        // Scroll the TextBox so the selection is visible
        EditorTextBox.ScrollToLine(
            EditorTextBox.GetLineIndexFromCharacterIndex(startIndex));
    }

    private void EditorView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not EditorViewModel vm) return;

        if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            vm.ToggleSearchCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (vm.UndoCommand.CanExecute(null))
            {
                vm.UndoCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (vm.RedoCommand.CanExecute(null))
            {
                vm.RedoCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Enter && IsSearchFocused())
        {
            // Enter in search box triggers Find Next
            if (vm.FindNextCommand.CanExecute(null))
            {
                vm.FindNextCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private bool IsSearchFocused()
    {
        var focused = Keyboard.FocusedElement as FrameworkElement;
        return focused != null && focused != EditorTextBox;
    }

    private void EditorTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && DataContext is EditorViewModel vm)
        {
            vm.SelectedText = textBox.SelectedText;
        }
    }

    private void EditorTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (DataContext is not EditorViewModel vm || EditorTextBox == null || !vm.IsSpellCheckEnabled)
            return;

        int caretIndex = EditorTextBox.CaretIndex;
        string text = EditorTextBox.Text;
        if (string.IsNullOrEmpty(text) || caretIndex < 0 || caretIndex > text.Length)
            return;

        // Find word boundaries under caret
        int start = caretIndex;
        while (start > 0 && char.IsLetterOrDigit(text[start - 1]))
            start--;

        int end = caretIndex;
        while (end < text.Length && char.IsLetterOrDigit(text[end]))
            end++;

        if (end <= start) return;

        string targetWord = text.Substring(start, end - start);
        var suggestions = vm.GetSuggestionsForWord(targetWord);

        var contextMenu = new ContextMenu();

        if (suggestions.Count > 0)
        {
            var headerItem = new MenuItem { Header = $"💡 Suggestions for '{targetWord}':", IsEnabled = false, FontWeight = FontWeights.Bold };
            contextMenu.Items.Add(headerItem);
            contextMenu.Items.Add(new Separator());

            foreach (string suggestion in suggestions)
            {
                string repl = suggestion;
                var item = new MenuItem { Header = $"✔ Use '{repl}'", FontWeight = FontWeights.SemiBold };
                item.Click += (s, args) =>
                {
                    vm.ApplySuggestionCommand.Execute(new System.Tuple<string, string>(targetWord, repl));
                };
                contextMenu.Items.Add(item);
            }
            contextMenu.Items.Add(new Separator());
        }

        var toggleItem = new MenuItem { Header = vm.IsSpellCheckEnabled ? "🔴 Disable Spell Check" : "🟢 Enable Spell Check" };
        toggleItem.Click += (s, args) => vm.ToggleSpellCheckCommand.Execute(null);
        contextMenu.Items.Add(toggleItem);

        EditorTextBox.ContextMenu = contextMenu;
    }
}
