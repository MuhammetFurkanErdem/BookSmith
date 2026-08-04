using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.UI.Commands;
using Microsoft.Win32;

namespace BookSmith.UI.ViewModels.Main;

public class EditorViewModel : ViewModelBase
{
    private readonly IEpubExporter? _epubExporter;

    private string _fileName = string.Empty;
    private string _cleanedText = string.Empty;
    private int _originalCharCount = 0;
    private int _cleanedCharCount = 0;
    private int _removedCharCount = 0;
    private double _reductionPercentage = 0;
    private string _statusText = "Ready to edit or export.";

    // Search & Replace state
    private string _searchQuery = string.Empty;
    private string _replaceQuery = string.Empty;
    private bool _isSearchPanelVisible = false;
    private int _searchResultCount = 0;
    private int _currentMatchIndex = -1;
    private string _searchDisplayText = string.Empty;

    /// <summary>Event fired when Find Next locates a match. Args: (startIndex, length)</summary>
    public event Action<int, int>? FindNextRequested;

    // Undo/Redo state
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();
    private const int MaxUndoHistory = 10;
    private bool _isUndoRedoOperation = false;

    // Selection state
    private string _selectedText = string.Empty;

    // Live counters
    private int _wordCount = 0;
    private int _lineCount = 0;

    // EPUB Customization State
    private string _coverImagePath = string.Empty;
    private string _authorName = "Unknown Author";
    private int _fontSizePt = 12;
    private bool _isEpubPanelVisible = false;

    // Chapter navigation
    private IReadOnlyList<ChapterInfo> _chapters = Array.Empty<ChapterInfo>();

    /// <summary>Event fired when a chapter is selected. Args: charOffset to scroll to.</summary>
    public event Action<int>? ScrollToChapterRequested;

    public Action? OnBackRequested { get; set; }

    #region Core Properties

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public string CleanedText
    {
        get => _cleanedText;
        set
        {
            string oldValue = _cleanedText;
            if (SetProperty(ref _cleanedText, value))
            {
                // Push to undo stack (only if not an undo/redo operation)
                if (!_isUndoRedoOperation && !string.IsNullOrEmpty(oldValue))
                {
                    if (_undoStack.Count >= MaxUndoHistory)
                    {
                        // Remove oldest items to maintain max size
                        var tempList = _undoStack.ToList();
                        tempList.RemoveAt(tempList.Count - 1);
                        _undoStack.Clear();
                        foreach (var item in tempList.AsEnumerable().Reverse())
                            _undoStack.Push(item);
                    }
                    _undoStack.Push(oldValue);
                    _redoStack.Clear();
                }

                // Update computed properties
                CleanedCharCount = _cleanedText.Length;
                RemovedCharCount = Math.Max(0, OriginalCharCount - CleanedCharCount);
                ReductionPercentage = OriginalCharCount > 0 ? (RemovedCharCount * 100.0 / OriginalCharCount) : 0;

                // Update live counters
                UpdateCounters();

                // Update search results if search is active
                if (IsSearchPanelVisible && !string.IsNullOrEmpty(SearchQuery))
                    UpdateSearchResultCount();

                // Notify CanExecute changes
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

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    #endregion

    #region Search & Replace Properties

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                UpdateSearchResultCount();
            }
        }
    }

    public string ReplaceQuery
    {
        get => _replaceQuery;
        set => SetProperty(ref _replaceQuery, value);
    }

    public bool IsSearchPanelVisible
    {
        get => _isSearchPanelVisible;
        set => SetProperty(ref _isSearchPanelVisible, value);
    }

    public int SearchResultCount
    {
        get => _searchResultCount;
        set => SetProperty(ref _searchResultCount, value);
    }

    public int CurrentMatchIndex
    {
        get => _currentMatchIndex;
        set => SetProperty(ref _currentMatchIndex, value);
    }

    public string SearchDisplayText
    {
        get => _searchDisplayText;
        set => SetProperty(ref _searchDisplayText, value);
    }

    #endregion

    #region Selection & Counter Properties

    public string SelectedText
    {
        get => _selectedText;
        set
        {
            if (SetProperty(ref _selectedText, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public int WordCount
    {
        get => _wordCount;
        set => SetProperty(ref _wordCount, value);
    }

    public int LineCount
    {
        get => _lineCount;
        set => SetProperty(ref _lineCount, value);
    }

    public IReadOnlyList<ChapterInfo> Chapters
    {
        get => _chapters;
        set
        {
            if (SetProperty(ref _chapters, value))
                OnPropertyChanged(nameof(HasChapters));
        }
    }

    public bool HasChapters => _chapters.Count > 0;

    #endregion

    #region EPUB Export Settings Properties

    public string CoverImagePath
    {
        get => _coverImagePath;
        set
        {
            if (SetProperty(ref _coverImagePath, value))
                OnPropertyChanged(nameof(HasCoverImage));
        }
    }

    public bool HasCoverImage => !string.IsNullOrWhiteSpace(CoverImagePath) && File.Exists(CoverImagePath);

    public string AuthorName
    {
        get => _authorName;
        set => SetProperty(ref _authorName, value);
    }

    public int FontSizePt
    {
        get => _fontSizePt;
        set => SetProperty(ref _fontSizePt, value);
    }

    public bool IsEpubPanelVisible
    {
        get => _isEpubPanelVisible;
        set => SetProperty(ref _isEpubPanelVisible, value);
    }

    #endregion

    #region Commands

    public ICommand ExportEpubCommand { get; }
    public ICommand ExportTxtCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand JumpToChapterCommand { get; }

    // EPUB customization commands
    public ICommand ToggleEpubPanelCommand { get; }
    public ICommand SelectCoverCommand { get; }
    public ICommand ClearCoverCommand { get; }

    // Search & Replace commands
    public ICommand ToggleSearchCommand { get; }
    public ICommand FindNextCommand { get; }
    public ICommand ReplaceCurrentCommand { get; }
    public ICommand ReplaceAllCommand { get; }

    // Quick-action commands
    public ICommand RemoveSelectedLinesCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }

    #endregion

    public EditorViewModel(IEpubExporter? epubExporter = null)
    {
        _epubExporter = epubExporter;

        // Export commands
        ExportEpubCommand = new RelayCommand(OnExportEpub, () => !string.IsNullOrWhiteSpace(CleanedText));
        ExportTxtCommand = new RelayCommand(OnExportTxt, () => !string.IsNullOrWhiteSpace(CleanedText));
        BackCommand = new RelayCommand(() => OnBackRequested?.Invoke());

        // Search & Replace commands
        ToggleSearchCommand = new RelayCommand(OnToggleSearch);
        FindNextCommand = new RelayCommand(OnFindNext, () => !string.IsNullOrWhiteSpace(SearchQuery) && SearchResultCount > 0);
        ReplaceCurrentCommand = new RelayCommand(OnReplaceCurrent, () => !string.IsNullOrWhiteSpace(SearchQuery) && SearchResultCount > 0);
        ReplaceAllCommand = new RelayCommand(OnReplaceAll, () => !string.IsNullOrWhiteSpace(SearchQuery) && SearchResultCount > 0);

        // Quick-action commands
        RemoveSelectedLinesCommand = new RelayCommand(OnRemoveSelectedLines, () => !string.IsNullOrWhiteSpace(SelectedText));
        UndoCommand = new RelayCommand(OnUndo, () => _undoStack.Count > 0);
        RedoCommand = new RelayCommand(OnRedo, () => _redoStack.Count > 0);

        // Chapter navigation command
        JumpToChapterCommand = new RelayCommand<ChapterInfo>(OnJumpToChapter);

        // EPUB customization commands
        ToggleEpubPanelCommand = new RelayCommand(() => IsEpubPanelVisible = !IsEpubPanelVisible);
        SelectCoverCommand = new RelayCommand(OnSelectCover);
        ClearCoverCommand = new RelayCommand(() => CoverImagePath = string.Empty);
    }

    #region Search & Replace Logic

    private void OnToggleSearch()
    {
        IsSearchPanelVisible = !IsSearchPanelVisible;
        if (!IsSearchPanelVisible)
        {
            SearchQuery = string.Empty;
            ReplaceQuery = string.Empty;
            SearchResultCount = 0;
        }
    }

    private void UpdateSearchResultCount()
    {
        if (string.IsNullOrEmpty(SearchQuery) || string.IsNullOrEmpty(CleanedText))
        {
            SearchResultCount = 0;
            CurrentMatchIndex = -1;
            SearchDisplayText = "";
            return;
        }

        int count = 0;
        int index = 0;
        while ((index = CleanedText.IndexOf(SearchQuery, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += SearchQuery.Length;
        }
        SearchResultCount = count;
        CurrentMatchIndex = count > 0 ? 0 : -1;
        SearchDisplayText = count > 0 ? $"1 of {count}" : "No matches";
    }

    private void OnFindNext()
    {
        if (string.IsNullOrEmpty(SearchQuery) || string.IsNullOrEmpty(CleanedText) || SearchResultCount == 0)
            return;

        // Find all match positions
        var positions = new System.Collections.Generic.List<int>();
        int idx = 0;
        while ((idx = CleanedText.IndexOf(SearchQuery, idx, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            positions.Add(idx);
            idx += SearchQuery.Length;
        }

        if (positions.Count == 0) return;

        // Advance to next match (wrap around)
        CurrentMatchIndex = (CurrentMatchIndex + 1) % positions.Count;
        SearchDisplayText = $"{CurrentMatchIndex + 1} of {positions.Count}";

        // Fire event so View can select the text in the TextBox
        FindNextRequested?.Invoke(positions[CurrentMatchIndex], SearchQuery.Length);
    }

    public void OnReplaceCurrent()
    {
        if (string.IsNullOrEmpty(SearchQuery) || string.IsNullOrEmpty(CleanedText))
            return;

        int index = CleanedText.IndexOf(SearchQuery, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            CleanedText = CleanedText.Substring(0, index) + (ReplaceQuery ?? "") + CleanedText.Substring(index + SearchQuery.Length);
            StatusText = $"Replaced 1 occurrence. {SearchResultCount} remaining.";
        }
    }

    public void OnReplaceAll()
    {
        if (string.IsNullOrEmpty(SearchQuery) || string.IsNullOrEmpty(CleanedText))
            return;

        int previousCount = SearchResultCount;
        CleanedText = Regex.Replace(CleanedText, Regex.Escape(SearchQuery), ReplaceQuery ?? "", RegexOptions.IgnoreCase);
        StatusText = $"Replaced {previousCount} occurrences.";
    }

    #endregion

    #region Quick-Action Logic

    public void OnRemoveSelectedLines()
    {
        if (string.IsNullOrWhiteSpace(SelectedText) || string.IsNullOrWhiteSpace(CleanedText))
            return;

        // Remove the selected text content from CleanedText
        string textToRemove = SelectedText.Trim();
        if (string.IsNullOrEmpty(textToRemove))
            return;

        int index = CleanedText.IndexOf(textToRemove, StringComparison.Ordinal);
        if (index >= 0)
        {
            // Remove the text and any trailing newline
            int endIndex = index + textToRemove.Length;
            if (endIndex < CleanedText.Length && CleanedText[endIndex] == '\n')
                endIndex++;
            else if (endIndex < CleanedText.Length - 1 && CleanedText[endIndex] == '\r' && CleanedText[endIndex + 1] == '\n')
                endIndex += 2;

            CleanedText = CleanedText.Substring(0, index) + CleanedText.Substring(endIndex);
            StatusText = "Removed selected lines.";
        }
    }

    public void OnUndo()
    {
        if (_undoStack.Count == 0) return;

        _isUndoRedoOperation = true;
        _redoStack.Push(_cleanedText);
        CleanedText = _undoStack.Pop();
        _isUndoRedoOperation = false;
        StatusText = "Undo performed.";
    }

    public void OnRedo()
    {
        if (_redoStack.Count == 0) return;

        _isUndoRedoOperation = true;
        _undoStack.Push(_cleanedText);
        CleanedText = _redoStack.Pop();
        _isUndoRedoOperation = false;
        StatusText = "Redo performed.";
    }

    #endregion

    #region Live Counters

    private System.Threading.CancellationTokenSource? _counterCts;

    private void UpdateCounters()
    {
        if (string.IsNullOrWhiteSpace(_cleanedText))
        {
            WordCount = 0;
            LineCount = 0;
            return;
        }

        string textToCount = _cleanedText;

        var app = System.Windows.Application.Current;
        if (app == null)
        {
            // Unit test environment: run synchronously
            int lines = 1;
            int words = 0;
            bool inWord = false;

            for (int i = 0; i < textToCount.Length; i++)
            {
                char c = textToCount[i];
                if (c == '\n') lines++;
                if (char.IsWhiteSpace(c)) inWord = false;
                else if (!inWord) { inWord = true; words++; }
            }

            WordCount = words;
            LineCount = lines;
            return;
        }

        _counterCts?.Cancel();
        _counterCts = new System.Threading.CancellationTokenSource();
        var token = _counterCts.Token;

        Task.Run(() =>
        {
            if (token.IsCancellationRequested) return;

            int lines = 1;
            int words = 0;
            bool inWord = false;

            for (int i = 0; i < textToCount.Length; i++)
            {
                if (token.IsCancellationRequested) return;

                char c = textToCount[i];
                if (c == '\n')
                    lines++;

                if (char.IsWhiteSpace(c))
                {
                    inWord = false;
                }
                else if (!inWord)
                {
                    inWord = true;
                    words++;
                }
            }

            if (!token.IsCancellationRequested)
            {
                int finalWords = words;
                int finalLines = lines;

                app.Dispatcher.InvokeAsync(() =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        WordCount = finalWords;
                        LineCount = finalLines;
                    }
                });
            }
        }, token);
    }

    #endregion

    #region Export Logic

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
                    Author = string.IsNullOrWhiteSpace(AuthorName) ? "Unknown Author" : AuthorName,
                    Language = "tr",
                    OutputPath = saveFileDialog.FileName,
                    ContentText = CleanedText,
                    Chapters = Chapters,
                    CoverImagePath = HasCoverImage ? CoverImagePath : null,
                    FontSizePt = FontSizePt
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

    #endregion

    #region Chapter Navigation

    private void OnJumpToChapter(ChapterInfo? chapter)
    {
        if (chapter == null) return;
        ScrollToChapterRequested?.Invoke(chapter.CharOffset);
    }

    private void OnSelectCover()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            Title = "Select Book Cover Image"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            CoverImagePath = openFileDialog.FileName;
        }
    }

    #endregion
}
