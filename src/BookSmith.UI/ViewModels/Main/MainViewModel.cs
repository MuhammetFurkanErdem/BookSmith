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

    private readonly IBatchProcessor? _batchProcessor;

    public MainViewModel(
        INavigationService navigationService,
        IPdfReader pdfReader,
        ITextCleaner textCleaner,
        IBookPipeline bookPipeline,
        ISettingsService? settingsService = null,
        IEpubExporter? epubExporter = null,
        IPresetManager? presetManager = null,
        IBatchProcessor? batchProcessor = null)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _bookPipeline = bookPipeline ?? throw new ArgumentNullException(nameof(bookPipeline));
        _batchProcessor = batchProcessor;

        ImportViewModel = new ImportViewModel(pdfReader, settingsService, presetManager);
        ProcessingViewModel = new ProcessingViewModel();
        EditorViewModel = new EditorViewModel(epubExporter);

        ImportViewModel.OnStartCleaningRequested = () => _ = OnStartCleaningAsync();
        EditorViewModel.OnBackRequested = () => _navigationService.NavigateTo(ImportViewModel, 1);

        _navigationService.NavigateTo(ImportViewModel, 1);
    }

    private async Task OnStartCleaningAsync()
    {
        if (ImportViewModel.IsBatchMode && _batchProcessor != null && ImportViewModel.SelectedFiles.Count > 0)
        {
            await RunBatchCleaningAsync();
            return;
        }

        if (string.IsNullOrWhiteSpace(ImportViewModel.FilePath))
            return;

        ProcessingViewModel.FileName = ImportViewModel.FileName;
        ProcessingViewModel.ProgressValue = 20;
        ProcessingViewModel.StatusText = "Extracting pages & cleaning book text...";

        _navigationService.NavigateTo(ProcessingViewModel, 2);

        try
        {
            string path = ImportViewModel.FilePath;
            bool removeFrontMatter = ImportViewModel.RemoveFrontMatter;
            var result = await Task.Run(() => _bookPipeline.Process(path, removeFrontMatter));

            ProcessingViewModel.ProgressValue = 80;
            ProcessingViewModel.StatusText = "Finalizing text cleaning...";

            EditorViewModel.FileName = ImportViewModel.FileName;
            EditorViewModel.CleanedText = result.CleanedText;
            EditorViewModel.OriginalCharCount = result.OriginalCharCount;
            EditorViewModel.CleanedCharCount = result.CleanedCharCount;
            EditorViewModel.RemovedCharCount = result.RemovedCharCount;
            EditorViewModel.ReductionPercentage = result.ReductionPercentage;
            EditorViewModel.Chapters = result.Chapters;
            EditorViewModel.StatusText = $"Completed! Cleaned {result.CleanedCharCount:N0} chars ({result.ReductionPercentage:F1}% reduction). {result.Chapters.Count} chapter(s) detected.";

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

    private async Task RunBatchCleaningAsync()
    {
        if (_batchProcessor == null) return;

        var files = ImportViewModel.SelectedFiles.ToList();
        var preset = ImportViewModel.SelectedPreset ?? new BookSmith.Core.Models.CleaningPreset
        {
            RemoveFrontMatter = ImportViewModel.RemoveFrontMatter
        };

        ProcessingViewModel.FileName = $"{files.Count} files (Batch Mode)";
        ProcessingViewModel.ProgressValue = 10;
        ProcessingViewModel.StatusText = "Starting batch queue processing...";

        _navigationService.NavigateTo(ProcessingViewModel, 2);

        try
        {
            var results = await _batchProcessor.ProcessBatchAsync(
                files,
                preset,
                (current, total, msg) =>
                {
                    int pct = (int)((double)current / total * 90.0);
                    ProcessingViewModel.ProgressValue = Math.Max(10, pct);
                    ProcessingViewModel.StatusText = msg;
                });

            var completed = results.Where(r => r.Status == BookSmith.Core.Models.BatchStatus.Completed).ToList();
            if (completed.Count > 0 && completed[0].Result != null)
            {
                var first = completed[0].Result!;
                EditorViewModel.FileName = completed[0].FileName;
                EditorViewModel.CleanedText = first.CleanedText;
                EditorViewModel.OriginalCharCount = first.OriginalCharCount;
                EditorViewModel.CleanedCharCount = first.CleanedCharCount;
                EditorViewModel.RemovedCharCount = first.RemovedCharCount;
                EditorViewModel.ReductionPercentage = first.ReductionPercentage;
                EditorViewModel.Chapters = first.Chapters;
                EditorViewModel.StatusText = $"Batch Completed! Processed {completed.Count}/{results.Count} files successfully.";
            }

            _navigationService.NavigateTo(EditorViewModel, 3);
        }
        catch (Exception ex)
        {
            ProcessingViewModel.StatusText = $"Batch Error: {ex.Message}";
            ProcessingViewModel.ProgressValue = 0;
            await Task.Delay(2000);
            _navigationService.NavigateTo(ImportViewModel, 1);
        }
    }
}
