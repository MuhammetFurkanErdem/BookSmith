using System;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.UI.Navigation;

namespace BookSmith.UI.ViewModels.Main;

public class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IBookPipeline _bookPipeline;

    public INavigationService NavigationService => _navigationService;
    public ImportViewModel ImportViewModel { get; }
    public ProcessingViewModel ProcessingViewModel { get; }
    public EditorViewModel EditorViewModel { get; }

    public MainViewModel(
        INavigationService navigationService,
        IPdfReader pdfReader,
        ITextCleaner textCleaner,
        IBookPipeline bookPipeline,
        ISettingsService? settingsService = null,
        IEpubExporter? epubExporter = null)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _bookPipeline = bookPipeline ?? throw new ArgumentNullException(nameof(bookPipeline));

        ImportViewModel = new ImportViewModel(pdfReader, settingsService);
        ProcessingViewModel = new ProcessingViewModel();
        EditorViewModel = new EditorViewModel(epubExporter);

        ImportViewModel.OnStartCleaningRequested = () => _ = OnStartCleaningAsync();
        EditorViewModel.OnBackRequested = () => _navigationService.NavigateTo(ImportViewModel, 1);

        _navigationService.NavigateTo(ImportViewModel, 1);
    }

    private async Task OnStartCleaningAsync()
    {
        if (string.IsNullOrWhiteSpace(ImportViewModel.FilePath))
            return;

        ProcessingViewModel.FileName = ImportViewModel.FileName;
        ProcessingViewModel.ProgressValue = 20;
        ProcessingViewModel.StatusText = "Extracting pages & cleaning book text...";

        _navigationService.NavigateTo(ProcessingViewModel, 2);

        try
        {
            string path = ImportViewModel.FilePath;
            var result = await Task.Run(() => _bookPipeline.Process(path));

            ProcessingViewModel.ProgressValue = 80;
            ProcessingViewModel.StatusText = "Finalizing text cleaning...";

            EditorViewModel.FileName = ImportViewModel.FileName;
            EditorViewModel.CleanedText = result.CleanedText;
            EditorViewModel.OriginalCharCount = result.OriginalCharCount;
            EditorViewModel.CleanedCharCount = result.CleanedCharCount;
            EditorViewModel.RemovedCharCount = result.RemovedCharCount;
            EditorViewModel.ReductionPercentage = result.ReductionPercentage;
            EditorViewModel.StatusText = $"Completed! Cleaned {result.CleanedCharCount:N0} chars ({result.ReductionPercentage:F1}% reduction).";

            _navigationService.NavigateTo(EditorViewModel, 3);
        }
        catch (Exception ex)
        {
            ProcessingViewModel.StatusText = $"Error processing book: {ex.Message}";
            ProcessingViewModel.ProgressValue = 0;
            await Task.Delay(2000);
            _navigationService.NavigateTo(ImportViewModel, 1);
        }
    }
}
