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

        string lineMerged = MergeWrappedLines(normalized);
        string cleanedText = FormatDialogueForTts(lineMerged);

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

    public string RemoveHeadersAndFooters(IReadOnlyList<string> pages)
    {
        if (pages == null || pages.Count == 0)
            return string.Empty;

        var headerCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var footerCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        var pageLinesList = new List<List<string>>(pages.Count);

        foreach (string rawPage in pages)
        {
            string pageText = (rawPage ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            string[] rawLines = pageText.Split('\n');
            var lines = new List<string>(rawLines.Length);
            foreach (string l in rawLines)
            {
                lines.Add(l);
            }
            pageLinesList.Add(lines);

            string? firstLine = GetFirstLine(lines);
            if (firstLine != null)
            {
                headerCounts[firstLine] = headerCounts.TryGetValue(firstLine, out int count) ? count + 1 : 1;
            }

            string? lastLine = GetLastLine(lines);
            if (lastLine != null)
            {
                footerCounts[lastLine] = footerCounts.TryGetValue(lastLine, out int count) ? count + 1 : 1;
            }
        }

        var headersToRemove = new HashSet<string>(StringComparer.Ordinal);
        foreach (var kvp in headerCounts)
        {
            if (kvp.Value >= 3)
            {
                headersToRemove.Add(kvp.Key);
            }
        }

        var footersToRemove = new HashSet<string>(StringComparer.Ordinal);
        foreach (var kvp in footerCounts)
        {
            if (kvp.Value >= 3)
            {
                footersToRemove.Add(kvp.Key);
            }
        }

        var cleanedPages = new List<string>();

        foreach (var lines in pageLinesList)
        {
            if (lines.Count == 0)
                continue;

            int firstIndex = GetFirstLineIndex(lines);
            if (firstIndex != -1)
            {
                string trimmed = lines[firstIndex].Trim();
                if (headersToRemove.Contains(trimmed))
                {
                    lines.RemoveAt(firstIndex);
                }
            }

            if (lines.Count > 0)
            {
                int lastIndex = GetLastLineIndex(lines);
                if (lastIndex != -1)
                {
                    string trimmed = lines[lastIndex].Trim();
                    if (footersToRemove.Contains(trimmed))
                    {
                        lines.RemoveAt(lastIndex);
                    }
                }
            }

            string pageContent = string.Join("\n", lines).Trim();
            if (!string.IsNullOrWhiteSpace(pageContent))
            {
                cleanedPages.Add(pageContent);
            }
        }

        return string.Join("\n\n", cleanedPages);
    }

    public string FormatDialogueForTts(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // 1. Normalize guillemets (« and ») to standard dialogue quotes (“ and ”)
        string normalizedQuotes = text
            .Replace('«', '“')
            .Replace('»', '”');

        string[] lines = normalizedQuotes.Split('\n');
        var result = new StringBuilder();
        int lineCount = lines.Length;

        for (int i = 0; i < lineCount; i++)
        {
            string line = lines[i].TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                result.Append('\n');
                continue;
            }

            string trimmedStart = line.TrimStart();
            if (trimmedStart.Length > 0)
            {
                char firstChar = trimmedStart[0];
                // Normalize leading dialogue dashes: '-' or '–' -> '— '
                if (firstChar == '-' || firstChar == '–')
                {
                    string lineBody = trimmedStart.Substring(1).TrimStart();
                    line = "— " + lineBody;
                }
                else if (firstChar == '—' && (trimmedStart.Length == 1 || trimmedStart[1] != ' '))
                {
                    string lineBody = trimmedStart.Substring(1).TrimStart();
                    line = "— " + lineBody;
                }
            }

            result.Append(line).Append('\n');
        }

        return result.ToString().Trim();
    }

    private static int GetFirstLineIndex(List<string> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
                return i;
        }
        return -1;
    }

    private static int GetLastLineIndex(List<string> lines)
    {
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
                return i;
        }
        return -1;
    }

    private static string? GetFirstLine(List<string> lines)
    {
        int index = GetFirstLineIndex(lines);
        return index != -1 ? lines[index].Trim() : null;
    }

    private static string? GetLastLine(List<string> lines)
    {
        int index = GetLastLineIndex(lines);
        return index != -1 ? lines[index].Trim() : null;
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
