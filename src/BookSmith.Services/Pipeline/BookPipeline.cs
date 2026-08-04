using System;
using System.IO;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Pipeline;

public class BookPipeline : IBookPipeline
{
    private readonly IPdfReader _pdfReader;
    private readonly ITextCleaner _textCleaner;
    private readonly IFrontMatterFilter? _frontMatterFilter;

    public BookPipeline(IPdfReader pdfReader, ITextCleaner textCleaner, IFrontMatterFilter? frontMatterFilter = null)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        _textCleaner = textCleaner ?? throw new ArgumentNullException(nameof(textCleaner));
        _frontMatterFilter = frontMatterFilter;
    }

    public BookProcessingResult Process(string filePath, bool removeFrontMatter = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);

        var pages = _pdfReader.ReadAllPages(filePath);
        string originalText = string.Join("\n\n", pages);

        // Step 1 (optional): Strip front-matter pages (publisher credits, ISBN, copyright)
        string textAfterFrontMatter;
        if (removeFrontMatter && _frontMatterFilter != null)
        {
            textAfterFrontMatter = _frontMatterFilter.FilterFrontMatter(pages);
        }
        else
        {
            textAfterFrontMatter = string.Join("\n\n", pages);
        }

        // Step 2: Remove running headers & footers
        // Re-split into page list for the header/footer remover
        var filteredPages = textAfterFrontMatter.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        string textWithoutHeadersFooters = _textCleaner.RemoveHeadersAndFooters(filteredPages);

        // Step 3: Full text cleaning (normalize, merge lines, TTS format, etc.)
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
