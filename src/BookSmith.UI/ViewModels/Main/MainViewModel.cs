using System.Windows.Input;
using BookSmith.UI.Commands;
using Microsoft.Win32;
using BookSmith.Core.Interfaces;

namespace BookSmith.UI.ViewModels.Main;

public class MainViewModel : ViewModelBase
{
    private readonly IPdfReader _pdfReader;

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

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (SetProperty(ref _filePath, value))
            {
                OnPropertyChanged(nameof(IsFileSelected));
                // Force command state evaluation
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

    public MainViewModel(IPdfReader pdfReader)
    {
        _pdfReader = pdfReader;
        BrowseCommand = new RelayCommand(OnBrowse);
        StartCleaningCommand = new RelayCommand(OnStartCleaning, CanStartCleaning);
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
            }
            catch (System.Exception ex)
            {
                StatusText = $"Error reading PDF: {ex.Message}";
                FileName = "Error loading metadata";
                FileSize = 0;
                PageCount = 0;
            }
        }
    }

    private bool CanStartCleaning()
    {
        return !string.IsNullOrWhiteSpace(FilePath);
    }

    private void OnStartCleaning()
    {
        StatusText = "Cleaning in progress...";
        ProgressValue = 50; // Visual proof of binding
    }
}
