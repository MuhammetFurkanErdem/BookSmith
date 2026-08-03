using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.Commands;
using Microsoft.Win32;

namespace BookSmith.UI.ViewModels.Main;

public class MainViewModel : ViewModelBase
{
    private readonly IPdfReader _pdfReader;
    private readonly ITextCleaner _textCleaner;
    private readonly IBookPipeline _bookPipeline;
    private readonly ISettingsService? _settingsService;
    private readonly IEpubExporter? _epubExporter;

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
        set
        {
            if (SetProperty(ref _isCleaned, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
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
        set
        {
            if (SetProperty(ref _removeHeaders, value))
                SaveSettings();
        }
    }

    public bool RemoveFooters
    {
        get => _removeFooters;
        set
        {
            if (SetProperty(ref _removeFooters, value))
                SaveSettings();
        }
    }

    public bool RemovePageNumbers
    {
        get => _removePageNumbers;
        set
        {
            if (SetProperty(ref _removePageNumbers, value))
                SaveSettings();
        }
    }

    public bool FixBrokenWords
    {
        get => _fixBrokenWords;
        set
        {
            if (SetProperty(ref _fixBrokenWords, value))
                SaveSettings();
        }
    }

    public bool MergeWrappedLines
    {
        get => _mergeWrappedLines;
        set
        {
            if (SetProperty(ref _mergeWrappedLines, value))
                SaveSettings();
        }
    }

    public bool SmartDialogueFormatting
    {
        get => _smartDialogueFormatting;
        set
        {
            if (SetProperty(ref _smartDialogueFormatting, value))
                SaveSettings();
        }
    }

    public bool ElevenReaderMode
    {
        get => _elevenReaderMode;
        set
        {
            if (SetProperty(ref _elevenReaderMode, value))
                SaveSettings();
        }
    }

    public bool ExportEpub
    {
        get => _exportEpub;
        set
        {
            if (SetProperty(ref _exportEpub, value))
                SaveSettings();
        }
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
    public ICommand ExportEpubCommand { get; }
    public ICommand ExportTxtCommand { get; }

    public MainViewModel(
        IPdfReader pdfReader,
        ITextCleaner textCleaner,
        IBookPipeline bookPipeline,
        ISettingsService? settingsService = null,
        IEpubExporter? epubExporter = null)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        _textCleaner = textCleaner ?? throw new ArgumentNullException(nameof(textCleaner));
        _bookPipeline = bookPipeline ?? throw new ArgumentNullException(nameof(bookPipeline));
        _settingsService = settingsService;
        _epubExporter = epubExporter;

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
        StartCleaningCommand = new AsyncRelayCommand(OnStartCleaningAsync, CanStartCleaning);
        ExportEpubCommand = new RelayCommand(OnExportEpub, CanExport);
        ExportTxtCommand = new RelayCommand(OnExportTxt, CanExport);
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

    private bool CanExport()
    {
        return IsCleaned && !string.IsNullOrWhiteSpace(CleanedText) && !IsProcessing;
    }

    public void OnExportEpub()
    {
        if (string.IsNullOrWhiteSpace(CleanedText))
            return;

        string defaultFileName = string.IsNullOrWhiteSpace(FileName)
            ? "CleanedBook.epub"
            : Path.GetFileNameWithoutExtension(FileName) + "_Cleaned.epub";

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "EPUB Book (*.epub)|*.epub",
            Title = "Export Cleaned Book as EPUB",
            FileName = defaultFileName
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var options = new EpubExportOptions
                {
                    Title = string.IsNullOrWhiteSpace(FileName) ? "Cleaned Book" : Path.GetFileNameWithoutExtension(FileName),
                    Author = "BookSmith",
                    Language = "tr",
                    OutputPath = saveFileDialog.FileName,
                    ContentText = CleanedText
                };

                if (_epubExporter != null)
                {
                    _epubExporter.Export(options);
                }
                else
                {
                    var exporter = new BookSmith.Services.Export.EpubExporter();
                    exporter.Export(options);
                }

                StatusText = $"Successfully exported EPUB to: {saveFileDialog.SafeFileName}";
            }
            catch (Exception ex)
            {
                StatusText = $"Error exporting EPUB: {ex.Message}";
            }
        }
    }

    public void OnExportTxt()
    {
        if (string.IsNullOrWhiteSpace(CleanedText))
            return;

        string defaultFileName = string.IsNullOrWhiteSpace(FileName)
            ? "CleanedBook.txt"
            : Path.GetFileNameWithoutExtension(FileName) + "_Cleaned.txt";

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Text Document (*.txt)|*.txt",
            Title = "Export Cleaned Book as TXT",
            FileName = defaultFileName
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(saveFileDialog.FileName, CleanedText, Encoding.UTF8);
                StatusText = $"Successfully exported TXT to: {saveFileDialog.SafeFileName}";
            }
            catch (Exception ex)
            {
                StatusText = $"Error exporting TXT: {ex.Message}";
            }
        }
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

            if (ExportEpub)
            {
                OnExportEpub();
            }
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
