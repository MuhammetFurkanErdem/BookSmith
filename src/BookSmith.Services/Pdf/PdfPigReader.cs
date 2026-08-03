using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace BookSmith.Services.Pdf;

public class PdfPigReader : IPdfReader
{
    public PdfMetadata ReadMetadata(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);

        var fileInfo = new FileInfo(filePath);
        var metadata = new PdfMetadata
        {
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length / 1024.0
        };

        using (var document = PdfDocument.Open(filePath))
        {
            metadata.PageCount = document.NumberOfPages;
        }

        return metadata;
    }

    public string ReadFirstPageText(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);

        using (var document = PdfDocument.Open(filePath))
        {
            if (document.NumberOfPages >= 1)
            {
                var page = document.GetPage(1);
                return ExtractPageText(page);
            }
            return string.Empty;
        }
    }

    public IReadOnlyList<string> ReadAllPages(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("PDF file not found.", filePath);

        var pages = new List<string>();

        using (var document = PdfDocument.Open(filePath))
        {
            for (int i = 1; i <= document.NumberOfPages; i++)
            {
                var page = document.GetPage(i);
                pages.Add(ExtractPageText(page));
            }
        }

        return pages;
    }

    private static string ExtractPageText(Page page)
    {
        try
        {
            var wordsList = page.GetWords()?.ToList();
            if (wordsList == null || wordsList.Count == 0)
            {
                return page.Text ?? string.Empty;
            }

            // Group words into lines using vertical Y-coordinate clustering
            var lines = new List<List<Word>>();
            double tolerance = 4.0; // Y-tolerance for words on the same line

            var sortedWords = wordsList
                .OrderByDescending(w => w.BoundingBox.Top)
                .ThenBy(w => w.BoundingBox.Left)
                .ToList();

            List<Word>? currentLine = null;
            double currentY = double.NaN;

            foreach (var word in sortedWords)
            {
                if (currentLine == null || Math.Abs(word.BoundingBox.Top - currentY) > tolerance)
                {
                    currentLine = new List<Word>();
                    lines.Add(currentLine);
                    currentY = word.BoundingBox.Top;
                }
                currentLine.Add(word);
            }

            var sb = new StringBuilder();
            foreach (var lineWords in lines)
            {
                var lineOrdered = lineWords.OrderBy(w => w.BoundingBox.Left);
                string lineText = string.Join(" ", lineOrdered.Select(w => w.Text));
                if (!string.IsNullOrWhiteSpace(lineText))
                {
                    sb.AppendLine(lineText.Trim());
                }
            }

            string resultText = sb.ToString().Trim();
            return !string.IsNullOrWhiteSpace(resultText) ? resultText : (page.Text ?? string.Empty);
        }
        catch
        {
            return page.Text ?? string.Empty;
        }
    }
}
