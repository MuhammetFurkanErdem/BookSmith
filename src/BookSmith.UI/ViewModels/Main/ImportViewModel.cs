using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
    private readonly IPresetManager? _presetManager;

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
    private bool _removeFrontMatter = true;

    // Presets & Batch Queue State
    private ObservableCollection<CleaningPreset> _presets = new();
    private CleaningPreset? _selectedPreset;
    private ObservableCollection<string> _selectedFiles = new();

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

    public bool RemoveFrontMatter
    {
        get => _removeFrontMatter;
        set { if (SetProperty(ref _removeFrontMatter, value)) SaveSettings(); }
    }

    public ObservableCollection<CleaningPreset> Presets
    {
        get => _presets;
        set => SetProperty(ref _presets, value);
    }

    public CleaningPreset? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (SetProperty(ref _selectedPreset, value) && value != null)
            {
                ApplyPreset(value);
            }
        }
    }

    public ObservableCollection<string> SelectedFiles
    {
        get => _selectedFiles;
        set
        {
            if (SetProperty(ref _selectedFiles, value))
            {
                OnPropertyChanged(nameof(IsBatchMode));
                OnPropertyChanged(nameof(FileCountText));
            }
        }
    }

    public bool IsBatchMode => SelectedFiles.Count > 1;

    public string FileCountText => SelectedFiles.Count > 1
        ? $"{SelectedFiles.Count} files selected (Batch Mode)"
        : IsFileSelected ? "1 file selected" : "No file selected";

    public ICommand BrowseCommand { get; }
    public ICommand StartCleaningCommand { get; }
    public ICommand SavePresetCommand { get; }

    public ImportViewModel(IPdfReader pdfReader, ISettingsService? settingsService = null, IPresetManager? presetManager = null)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        _settingsService = settingsService;
        _presetManager = presetManager;

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
            _removeFrontMatter = saved.RemoveFrontMatter;
        }

        LoadPresets();

        BrowseCommand = new RelayCommand(OnBrowse);
        StartCleaningCommand = new RelayCommand(OnStartCleaning, () => IsFileSelected || SelectedFiles.Count > 0);
        SavePresetCommand = new RelayCommand(OnSavePreset);
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
            ExportEpub = ExportEpub,
            RemoveFrontMatter = RemoveFrontMatter
        };
        _settingsService.SaveSettings(settings);
    }

    private void LoadPresets()
    {
        if (_presetManager == null) return;
        var list = _presetManager.GetAllPresets();
        Presets = new ObservableCollection<CleaningPreset>(list);
        _selectedPreset = Presets.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedPreset));
    }

    private void ApplyPreset(CleaningPreset preset)
    {
        _removeHeaders = preset.RemoveHeaders;
        _removeFooters = preset.RemoveFooters;
        _removePageNumbers = preset.RemovePageNumbers;
        _fixBrokenWords = preset.FixBrokenWords;
        _mergeWrappedLines = preset.MergeWrappedLines;
        _smartDialogueFormatting = preset.SmartDialogueFormatting;
        _elevenReaderMode = preset.ElevenReaderMode;
        _exportEpub = preset.ExportEpub;
        _removeFrontMatter = preset.RemoveFrontMatter;

        OnPropertyChanged(nameof(RemoveHeaders));
        OnPropertyChanged(nameof(RemoveFooters));
        OnPropertyChanged(nameof(RemovePageNumbers));
        OnPropertyChanged(nameof(FixBrokenWords));
        OnPropertyChanged(nameof(MergeWrappedLines));
        OnPropertyChanged(nameof(SmartDialogueFormatting));
        OnPropertyChanged(nameof(ElevenReaderMode));
        OnPropertyChanged(nameof(ExportEpub));
        OnPropertyChanged(nameof(RemoveFrontMatter));
    }

    private void OnSavePreset()
    {
        if (_presetManager == null) return;

        string name = $"Custom Preset {Presets.Count(p => !p.IsBuiltIn) + 1}";
        var newPreset = new CleaningPreset
        {
            Name = name,
            Description = "User-created custom cleaning rules preset.",
            IsBuiltIn = false,
            RemoveHeaders = RemoveHeaders,
            RemoveFooters = RemoveFooters,
            RemovePageNumbers = RemovePageNumbers,
            FixBrokenWords = FixBrokenWords,
            MergeWrappedLines = MergeWrappedLines,
            SmartDialogueFormatting = SmartDialogueFormatting,
            ElevenReaderMode = ElevenReaderMode,
            ExportEpub = ExportEpub,
            RemoveFrontMatter = RemoveFrontMatter
        };

        _presetManager.SavePreset(newPreset);
        LoadPresets();
        SelectedPreset = Presets.FirstOrDefault(p => p.Id == newPreset.Id);
    }

    private void OnBrowse()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            Title = "Select Audiobook PDF File(s)",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            SelectedFiles = new ObservableCollection<string>(openFileDialog.FileNames);

            if (openFileDialog.FileNames.Length > 0)
            {
                FilePath = openFileDialog.FileNames[0];

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
    }

    private void OnStartCleaning()
    {
        OnStartCleaningRequested?.Invoke();
    }
}
