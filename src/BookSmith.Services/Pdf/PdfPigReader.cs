using System;
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
}
