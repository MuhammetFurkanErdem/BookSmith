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
}
