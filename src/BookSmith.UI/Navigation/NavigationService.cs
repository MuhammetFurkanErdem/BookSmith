using System;
using BookSmith.UI.ViewModels;

namespace BookSmith.UI.Navigation;

public class NavigationService : INavigationService
{
    private ViewModelBase? _currentViewModel;
    private int _currentStep = 1;

    public ViewModelBase? CurrentViewModel => _currentViewModel;
    public int CurrentStep => _currentStep;

    public event Action? CurrentViewModelChanged;

    public void NavigateTo(ViewModelBase viewModel, int stepNumber)
    {
        _currentViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _currentStep = stepNumber;
        CurrentViewModelChanged?.Invoke();
    }
}
