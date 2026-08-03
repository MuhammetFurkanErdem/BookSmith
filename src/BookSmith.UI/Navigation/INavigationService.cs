using System;
using BookSmith.UI.ViewModels;

namespace BookSmith.UI.Navigation;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    int CurrentStep { get; }
    event Action? CurrentViewModelChanged;

    void NavigateTo(ViewModelBase viewModel, int stepNumber);
}
