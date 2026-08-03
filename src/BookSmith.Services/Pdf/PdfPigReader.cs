using System;
using System.Collections.Generic;
using System.IO;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using UglyToad.PdfPig;

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
                return page.Text ?? string.Empty;
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
                pages.Add(page.Text ?? string.Empty);
            }
        }

        return pages;
    }
}
