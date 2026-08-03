using System.Collections.Generic;
using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

public interface IPdfReader
{
    PdfMetadata ReadMetadata(string filePath);
    string ReadFirstPageText(string filePath);
    IReadOnlyList<string> ReadAllPages(string filePath);
}
