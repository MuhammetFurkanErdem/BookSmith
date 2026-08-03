using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookSmith.UI.ViewModels.Main;

namespace BookSmith.UI.Views;

public partial class EditorView : UserControl
{
    public EditorView()
    {
        InitializeComponent();
        PreviewKeyDown += EditorView_PreviewKeyDown;
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
    }

    private void EditorTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && DataContext is EditorViewModel vm)
        {
            vm.SelectedText = textBox.SelectedText;
        }
    }
}
