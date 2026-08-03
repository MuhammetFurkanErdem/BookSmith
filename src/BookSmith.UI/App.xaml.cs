using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookSmith.Core.Interfaces;
using BookSmith.Services.Pdf;
using BookSmith.Services.Cleaning;
using BookSmith.Services.Pipeline;
using BookSmith.UI.ViewModels.Main;
using BookSmith.UI.Shell;

namespace BookSmith.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register Core/Service layers
                services.AddSingleton<IPdfReader, PdfPigReader>();
                services.AddSingleton<ITextCleaner, TextCleaner>();
                services.AddSingleton<IBookPipeline, BookPipeline>();

                // Register UI layers (ViewModels and Views)
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var viewModel = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = viewModel;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
