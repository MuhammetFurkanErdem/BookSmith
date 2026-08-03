using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BookSmith.Core.Interfaces;
using BookSmith.UI.Commands;
using Microsoft.Win32;

namespace BookSmith.UI.ViewModels.Main;

public class MainViewModel : ViewModelBase
{
    private readonly IPdfReader _pdfReader;
    private readonly ITextCleaner _textCleaner;
    private readonly IBookPipeline _bookPipeline;

    private string _filePath = string.Empty;
    private bool _removeHeaders = true;
    private bool _removeFooters = true;
    private bool _removePageNumbers = true;
    private bool _fixBrokenWords = true;
    private bool _mergeWrappedLines = true;
    private bool _smartDialogueFormatting = true;
    private bool _elevenReaderMode = false;
    private bool _exportEpub = false;
    private double _progressValue = 0;
    private string _statusText = "Ready.";
    private string _fileName = string.Empty;
    private double _fileSize;
    private int _pageCount;
    private string _previewText = string.Empty;
    private string _cleanedText = string.Empty;
    private bool _isProcessing = false;
    private bool _isCleaned = false;
    private int _originalCharCount = 0;
    private int _cleanedCharCount = 0;
    private int _removedCharCount = 0;
    private double _reductionPercentage = 0;

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (SetProperty(ref _filePath, value))
            {
                OnPropertyChanged(nameof(IsFileSelected));
                IsCleaned = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsFileSelected => !string.IsNullOrWhiteSpace(FilePath);

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public double FileSize
    {
        get => _fileSize;
        set => SetProperty(ref _fileSize, value);
    }

    public int PageCount
    {
        get => _pageCount;
        set => SetProperty(ref _pageCount, value);
    }

    public string PreviewText
    {
        get => _previewText;
        set => SetProperty(ref _previewText, value);
    }

    public string CleanedText
    {
        get => _cleanedText;
        set => SetProperty(ref _cleanedText, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (SetProperty(ref _isProcessing, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsCleaned
    {
        get => _isCleaned;
        set => SetProperty(ref _isCleaned, value);
    }

    public int OriginalCharCount
    {
        get => _originalCharCount;
        set => SetProperty(ref _originalCharCount, value);
    }

    public int CleanedCharCount
    {
        get => _cleanedCharCount;
        set => SetProperty(ref _cleanedCharCount, value);
    }

    public int RemovedCharCount
    {
        get => _removedCharCount;
        set => SetProperty(ref _removedCharCount, value);
    }

    public double ReductionPercentage
    {
        get => _reductionPercentage;
        set => SetProperty(ref _reductionPercentage, value);
    }

    public bool RemoveHeaders
    {
        get => _removeHeaders;
        set => SetProperty(ref _removeHeaders, value);
    }

    public bool RemoveFooters
    {
        get => _removeFooters;
        set => SetProperty(ref _removeFooters, value);
    }

    public bool RemovePageNumbers
    {
        get => _removePageNumbers;
        set => SetProperty(ref _removePageNumbers, value);
    }

    public bool FixBrokenWords
    {
        get => _fixBrokenWords;
        set => SetProperty(ref _fixBrokenWords, value);
    }

    public bool MergeWrappedLines
    {
        get => _mergeWrappedLines;
        set => SetProperty(ref _mergeWrappedLines, value);
    }

    public bool SmartDialogueFormatting
    {
        get => _smartDialogueFormatting;
        set => SetProperty(ref _smartDialogueFormatting, value);
    }

    public bool ElevenReaderMode
    {
        get => _elevenReaderMode;
        set => SetProperty(ref _elevenReaderMode, value);
    }

    public bool ExportEpub
    {
        get => _exportEpub;
        set => SetProperty(ref _exportEpub, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public ICommand BrowseCommand { get; }
    public ICommand StartCleaningCommand { get; }

    public MainViewModel(IPdfReader pdfReader, ITextCleaner textCleaner, IBookPipeline bookPipeline)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        _textCleaner = textCleaner ?? throw new ArgumentNullException(nameof(textCleaner));
        _bookPipeline = bookPipeline ?? throw new ArgumentNullException(nameof(bookPipeline));

        BrowseCommand = new RelayCommand(OnBrowse);
        StartCleaningCommand = new AsyncRelayCommand(OnStartCleaningAsync, CanStartCleaning);
    }

    private void OnBrowse()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            Title = "Select Audiobook PDF File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            FilePath = openFileDialog.FileName;
            StatusText = $"Selected file: {openFileDialog.SafeFileName}";

            try
            {
                var metadata = _pdfReader.ReadMetadata(FilePath);
                FileName = metadata.FileName;
                FileSize = metadata.FileSize;
                PageCount = metadata.PageCount;
                PreviewText = _pdfReader.ReadFirstPageText(FilePath);
            }
            catch (Exception ex)
            {
                StatusText = $"Error reading PDF: {ex.Message}";
                FileName = "Error loading metadata";
                FileSize = 0;
                PageCount = 0;
                PreviewText = "Error reading page content.";
            }
        }
    }

    private bool CanStartCleaning()
    {
        return !string.IsNullOrWhiteSpace(FilePath) && !IsProcessing;
    }

    private async Task OnStartCleaningAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        IsProcessing = true;
        IsCleaned = false;
        ProgressValue = 20;
        StatusText = "Extracting pages & cleaning book text...";

        try
        {
            string path = FilePath;
            var result = await Task.Run(() => _bookPipeline.Process(path));

            ProgressValue = 80;
            StatusText = "Finalizing text cleaning...";

            CleanedText = result.CleanedText;
            OriginalCharCount = result.OriginalCharCount;
            CleanedCharCount = result.CleanedCharCount;
            RemovedCharCount = result.RemovedCharCount;
            ReductionPercentage = result.ReductionPercentage;

            ProgressValue = 100;
            IsCleaned = true;
            StatusText = $"Completed! Cleaned {CleanedCharCount:N0} chars ({ReductionPercentage:F1}% reduction).";
        }
        catch (Exception ex)
        {
            StatusText = $"Error processing book: {ex.Message}";
            ProgressValue = 0;
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
