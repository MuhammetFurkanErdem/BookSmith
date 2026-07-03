using System.Windows.Input;
using BookSmith.UI.Commands;
using Microsoft.Win32;

namespace BookSmith.UI.ViewModels.Main;

public class MainViewModel : ViewModelBase
{
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

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (SetProperty(ref _filePath, value))
            {
                // Force command state evaluation
                CommandManager.InvalidateRequerySuggested();
            }
        }
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

    public MainViewModel()
    {
        BrowseCommand = new RelayCommand(OnBrowse);
        StartCleaningCommand = new RelayCommand(OnStartCleaning, CanStartCleaning);
    }

    private void OnBrowse()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
            Title = "Select Audiobook PDF File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            FilePath = openFileDialog.FileName;
            StatusText = $"Selected file: {openFileDialog.SafeFileName}";
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
