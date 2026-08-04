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
    private readonly IChapterDetector? _chapterDetector;

    public BookPipeline(
        IPdfReader pdfReader,
        ITextCleaner textCleaner,
        IFrontMatterFilter? frontMatterFilter = null,
        IChapterDetector? chapterDetector = null)
    {
        _pdfReader = pdfReader ?? throw new ArgumentNullException(nameof(pdfReader));
        _textCleaner = textCleaner ?? throw new ArgumentNullException(nameof(textCleaner));
        _frontMatterFilter = frontMatterFilter;
        _chapterDetector = chapterDetector;
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
        IReadOnlyList<string> pagesAfterFrontMatter;
        if (removeFrontMatter && _frontMatterFilter != null)
        {
            pagesAfterFrontMatter = _frontMatterFilter.FilterFrontMatterPages(pages);
        }
        else
        {
            pagesAfterFrontMatter = pages;
        }

        // Step 2: Remove running headers & footers on true page list
        string textWithoutHeadersFooters = _textCleaner.RemoveHeadersAndFooters(pagesAfterFrontMatter);

        // Step 3: Full text cleaning (normalize, merge lines, TTS format, etc.)
        var cleaningResult = _textCleaner.Clean(textWithoutHeadersFooters);
        string cleanedText = cleaningResult.CleanedText;

        // Step 4 (optional): Detect chapter headings
        var chapters = _chapterDetector?.DetectChapters(cleanedText)
                       ?? System.Array.Empty<ChapterInfo>();

        return new BookProcessingResult
        {
            FilePath = filePath,
            TotalPages = pages.Count,
            OriginalText = originalText,
            CleanedText = cleanedText,
            Chapters = chapters
        };
    }
}
