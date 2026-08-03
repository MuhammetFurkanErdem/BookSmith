using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BookSmith.UI.ViewModels;

namespace BookSmith.UI.Navigation;

public class NavigationService : INavigationService, INotifyPropertyChanged
{
    private ViewModelBase? _currentViewModel;
    private int _currentStep = 1;

    public ViewModelBase? CurrentViewModel => _currentViewModel;
    public int CurrentStep => _currentStep;

    public event Action? CurrentViewModelChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void NavigateTo(ViewModelBase viewModel, int stepNumber)
    {
        _currentViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _currentStep = stepNumber;
        CurrentViewModelChanged?.Invoke();
        OnPropertyChanged(nameof(CurrentViewModel));
        OnPropertyChanged(nameof(CurrentStep));
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
