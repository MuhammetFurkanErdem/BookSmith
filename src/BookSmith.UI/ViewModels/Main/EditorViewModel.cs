using System;
using System.IO;
using System.Text;
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

    public Action? OnBackRequested { get; set; }

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
            if (SetProperty(ref _cleanedText, value))
            {
                CleanedCharCount = _cleanedText.Length;
                RemovedCharCount = Math.Max(0, OriginalCharCount - CleanedCharCount);
                ReductionPercentage = OriginalCharCount > 0 ? (RemovedCharCount * 100.0 / OriginalCharCount) : 0;
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

    public ICommand ExportEpubCommand { get; }
    public ICommand ExportTxtCommand { get; }
    public ICommand BackCommand { get; }

    public EditorViewModel(IEpubExporter? epubExporter = null)
    {
        _epubExporter = epubExporter;

        ExportEpubCommand = new RelayCommand(OnExportEpub, () => !string.IsNullOrWhiteSpace(CleanedText));
        ExportTxtCommand = new RelayCommand(OnExportTxt, () => !string.IsNullOrWhiteSpace(CleanedText));
        BackCommand = new RelayCommand(() => OnBackRequested?.Invoke());
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
}
