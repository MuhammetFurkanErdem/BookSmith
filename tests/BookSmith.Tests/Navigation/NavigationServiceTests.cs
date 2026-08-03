using System;
using BookSmith.UI.Navigation;
using BookSmith.UI.ViewModels;
using Xunit;

namespace BookSmith.Tests.Navigation;

public class NavigationServiceTests
{
    private class DummyViewModel : ViewModelBase
    {
    }

    [Fact]
    public void NavigateTo_UpdatesCurrentViewModelAndStep_AndFiresEvent()
    {
        var service = new NavigationService();
        var dummyVm = new DummyViewModel();
        bool eventFired = false;

        service.CurrentViewModelChanged += () => eventFired = true;

        service.NavigateTo(dummyVm, 2);

        Assert.Same(dummyVm, service.CurrentViewModel);
        Assert.Equal(2, service.CurrentStep);
        Assert.True(eventFired);
    }
}
