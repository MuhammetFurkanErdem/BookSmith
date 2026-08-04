using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookSmith.Core.Interfaces;
using BookSmith.Services.Pdf;
using BookSmith.Services.Cleaning;
using BookSmith.Services.Pipeline;
using BookSmith.Services.Export;
using BookSmith.Infrastructure.Settings;
using BookSmith.UI.Navigation;
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
                services.AddSingleton<IFrontMatterFilter, FrontMatterFilter>();
                services.AddSingleton<IChapterDetector, ChapterDetector>();
                services.AddSingleton<ISpellChecker, TurkishSpellChecker>();
                services.AddSingleton<IBookPipeline, BookPipeline>();
                services.AddSingleton<IBatchProcessor, BatchProcessor>();
                services.AddSingleton<IEpubExporter, EpubExporter>();
                services.AddSingleton<ISettingsService, JsonSettingsService>();
                services.AddSingleton<IPresetManager, JsonPresetManager>();
                services.AddSingleton<INavigationService, NavigationService>();

                // Register UI layers (ViewModels and Views)
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"An unexpected UI error occurred:\n{args.Exception.Message}", "BookSmith Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var viewModel = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = viewModel;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(2));
            }
        }
        catch
        {
            // Swallow any shutdown exceptions
        }
        finally
        {
            base.OnExit(e);
            // Force-terminate the process to release all DLL file locks
            System.Environment.Exit(0);
        }
    }
}
