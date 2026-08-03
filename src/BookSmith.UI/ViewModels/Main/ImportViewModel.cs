using System;
using System.Windows.Input;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.Commands;
using Microsoft.Win32;

namespace BookSmith.UI.ViewModels.Main;

public class ImportViewModel : ViewModelBase
{
    private readonly IPdfReader _pdfReader;
    private readonly ISettingsService? _settingsService;

    private string _filePath = string.Empty;
    private string _fileName = string.Empty;
    private double _fileSize;
    private int _pageCount;
    private string _previewText = string.Empty;

    private bool _removeHeaders = true;
    private bool _removeFooters = true;
    private bool _removePageNumbers = true;
    private bool _fixBrokenWords = true;
    private bool _mergeWrappedLines = true;
    private bool _smartDialogueFormatting = true;
    private bool _elevenReaderMode = false;
    private bool _exportEpub = false;

    public Action? OnStartCleaningRequested { get; set; }

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (SetProperty(ref _filePath, value))
            {
                OnPropertyChanged(nameof(IsFileSelected));
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

    public bool RemoveHeaders
    {
        get => _removeHeaders;
        set { if (SetProperty(ref _removeHeaders, value)) SaveSettings(); }
    }

    public bool RemoveFooters
    {
        get => _removeFooters;
        set { if (SetProperty(ref _removeFooters, value)) SaveSettings(); }
    }

    public bool RemovePageNumbers
    {
        get => _removePageNumbers;
        set { if (SetProperty(ref _removePageNumbers, value)) SaveSettings(); }
    }

    public bool FixBrokenWords
    {
        get => _fixBrokenWords;
        set { if (SetProperty(ref _fixBrokenWords, value)) SaveSettings(); }
    }

    public bool MergeWrappedLines
    {
        get => _mergeWrappedLines;
        set { if (SetProperty(ref _mergeWrappedLines, value)) SaveSettings(); }
    }

    public bool SmartDialogueFormatting
    {
        get => _smartDialogueFormatting;
        set { if (SetProperty(ref _smartDialogueFormatting, value)) SaveSettings(); }
    }

    public bool ElevenReaderMode
    {
        get => _elevenReaderMode;
        set { if (SetProperty(ref _elevenReaderMode, value)) SaveSettings(); }
    }

    public bool ExportEpub
    {
        get => _exportEpub;
        set { if (SetProperty(ref _exportEpub, value)) SaveSettings(); }
    }

    public ICommand BrowseCommand { get; }
    public ICommand StartCleaningCommand { get; }

    public ImportViewModel(IPdfReader pdfReader, ISettingsService? settingsService = null)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        _settingsService = settingsService;

        if (_settingsService != null)
        {
            var saved = _settingsService.LoadSettings();
            _removeHeaders = saved.RemoveHeaders;
            _removeFooters = saved.RemoveFooters;
            _removePageNumbers = saved.RemovePageNumbers;
            _fixBrokenWords = saved.FixBrokenWords;
            _mergeWrappedLines = saved.MergeWrappedLines;
            _smartDialogueFormatting = saved.SmartDialogueFormatting;
            _elevenReaderMode = saved.ElevenReaderMode;
            _exportEpub = saved.ExportEpub;
        }

        BrowseCommand = new RelayCommand(OnBrowse);
        StartCleaningCommand = new RelayCommand(OnStartCleaning, () => IsFileSelected);
    }

    private void SaveSettings()
    {
        if (_settingsService == null) return;
        var settings = new AppSettings
        {
            RemoveHeaders = RemoveHeaders,
            RemoveFooters = RemoveFooters,
            RemovePageNumbers = RemovePageNumbers,
            FixBrokenWords = FixBrokenWords,
            MergeWrappedLines = MergeWrappedLines,
            SmartDialogueFormatting = SmartDialogueFormatting,
            ElevenReaderMode = ElevenReaderMode,
            ExportEpub = ExportEpub
        };
        _settingsService.SaveSettings(settings);
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

            try
            {
                var metadata = _pdfReader.ReadMetadata(FilePath);
                FileName = metadata.FileName;
                FileSize = metadata.FileSize;
                PageCount = metadata.PageCount;
                PreviewText = _pdfReader.ReadFirstPageText(FilePath);
            }
            catch
            {
                FileName = "Error loading metadata";
                FileSize = 0;
                PageCount = 0;
                PreviewText = "Error reading page content.";
            }
        }
    }

    private void OnStartCleaning()
    {
        OnStartCleaningRequested?.Invoke();
    }
}
