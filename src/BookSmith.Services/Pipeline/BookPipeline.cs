using System;
using System.IO;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Pipeline;

public class BookPipeline : IBookPipeline
{
    private readonly IPdfReader _pdfReader;
    private readonly ITextCleaner _textCleaner;

    public BookPipeline(IPdfReader pdfReader, ITextCleaner textCleaner)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        _textCleaner = textCleaner ?? throw new ArgumentNullException(nameof(textCleaner));
    }

    public BookProcessingResult Process(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);

        var pages = _pdfReader.ReadAllPages(filePath);
        string originalText = string.Join("\n\n", pages);

        string textWithoutHeadersFooters = _textCleaner.RemoveHeadersAndFooters(pages);
        var cleaningResult = _textCleaner.Clean(textWithoutHeadersFooters);

        return new BookProcessingResult
        {
            FilePath = filePath,
            TotalPages = pages.Count,
            OriginalText = originalText,
            CleanedText = cleaningResult.CleanedText
        };
    }
}
