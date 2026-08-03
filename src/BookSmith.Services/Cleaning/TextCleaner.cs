using System;
using System.Collections.Generic;
using System.Text;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Cleaning;

public class TextCleaner : ITextCleaner
{
    public TextCleaningResult Clean(string input)
    {
        string original = input ?? string.Empty;

        string normalized = original
            .Replace("\u00AD", string.Empty)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        string cleanedText = MergeWrappedLines(normalized);

        return new TextCleaningResult
        {
            OriginalText = original,
            CleanedText = cleanedText
        };
    }

    public string MergeWrappedLines(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string[] lines = text.Split('\n');
        var result = new StringBuilder();
        int lineCount = lines.Length;

        for (int i = 0; i < lineCount; i++)
        {
            string currentLine = lines[i].TrimEnd();

            if (string.IsNullOrWhiteSpace(currentLine))
            {
                result.Append('\n');
                continue;
            }

            if (i < lineCount - 1)
            {
                string nextLine = lines[i + 1].TrimEnd();

                if (!string.IsNullOrWhiteSpace(nextLine) &&
                    !EndsWithSentencePunctuation(currentLine) &&
                    !IsDialogueLine(nextLine))
                {
                    result.Append(currentLine);
                    result.Append(' ');
                    continue;
                }
            }

            result.Append(currentLine).Append('\n');
        }

        return result.ToString().Trim();
    }

    private static bool EndsWithSentencePunctuation(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        string trimmed = line.TrimEnd(' ', '\t', '"', '“', '”', '’', '\'');
        if (trimmed.Length == 0) return false;

        char lastChar = trimmed[trimmed.Length - 1];
        return lastChar == '.' || lastChar == '?' || lastChar == '!' || lastChar == ':';
    }

    private static bool IsDialogueLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        char firstChar = line.TrimStart()[0];
        return firstChar == '-' || firstChar == '—' || firstChar == '–' ||
               firstChar == '"' || firstChar == '“' || firstChar == '‘' || firstChar == '«';
    }
}
