using System;
using System.Collections.Generic;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Cleaning;

public class BasicTextCleaner : ITextCleaner
{
    public TextCleaningResult Clean(string input)
    {
        string original = input ?? string.Empty;
        string normalized = original
            .Replace("\u00AD", string.Empty)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        string[] lines = normalized.Split('\n');
        var resultLines = new List<string>(lines.Length);
        int consecutiveBlankCount = 0;

        foreach (string line in lines)
        {
            string trimmedLine = line.TrimEnd();
            if (trimmedLine.Length == 0)
            {
                consecutiveBlankCount++;
                if (consecutiveBlankCount <= 2)
                {
                    resultLines.Add(trimmedLine);
                }
            }
            else
            {
                consecutiveBlankCount = 0;
                resultLines.Add(trimmedLine);
            }
        }

        string cleaned = string.Join("\n", resultLines).Trim();

        return new TextCleaningResult
        {
            OriginalText = original,
            CleanedText = cleaned
        };
    }
}
