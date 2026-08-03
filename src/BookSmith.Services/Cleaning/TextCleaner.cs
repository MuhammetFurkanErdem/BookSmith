using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

        string disjoined = DisjoinGluedHeadersAndNumbers(normalized);
        string lineMerged = MergeWrappedLines(disjoined);
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

            // Discard standalone page numbers during line merging
            if (IsPageNumberLine(currentLine))
            {
                continue;
            }

            if (i < lineCount - 1)
            {
                string nextLine = lines[i + 1].TrimEnd();

                if (!string.IsNullOrWhiteSpace(nextLine) &&
                    !IsPageNumberLine(nextLine) &&
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

        var headerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var footerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var pageLinesList = new List<List<string>>(pages.Count);

        foreach (string rawPage in pages)
        {
            string pageText = DisjoinGluedHeadersAndNumbers(rawPage ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            string[] rawLines = pageText.Split('\n');
            var lines = new List<string>(rawLines.Length);
            foreach (string l in rawLines)
            {
                lines.Add(l);
            }
            pageLinesList.Add(lines);

            // Check top 2 non-empty lines for potential running headers
            var topCandidates = GetTopNonEmptyLines(lines, 2);
            foreach (var candidate in topCandidates)
            {
                string trimmed = candidate.text.Trim();
                if (!IsPageNumberLine(trimmed))
                {
                    headerCounts[trimmed] = headerCounts.TryGetValue(trimmed, out int count) ? count + 1 : 1;
                }
            }

            // Check bottom 2 non-empty lines for potential running footers
            var bottomCandidates = GetBottomNonEmptyLines(lines, 2);
            foreach (var candidate in bottomCandidates)
            {
                string trimmed = candidate.text.Trim();
                if (!IsPageNumberLine(trimmed))
                {
                    footerCounts[trimmed] = footerCounts.TryGetValue(trimmed, out int count) ? count + 1 : 1;
                }
            }
        }

        // Header/footer candidates appearing 2+ times across pages
        var headersToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in headerCounts)
        {
            if (kvp.Value >= 2)
            {
                headersToRemove.Add(kvp.Key);
            }
        }

        var footersToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in footerCounts)
        {
            if (kvp.Value >= 2)
            {
                footersToRemove.Add(kvp.Key);
            }
        }

        var cleanedPages = new List<string>();

        foreach (var lines in pageLinesList)
        {
            if (lines.Count == 0)
                continue;

            // Strip top page numbers and headers
            for (int k = 0; k < 3; k++)
            {
                int topIndex = GetFirstLineIndex(lines);
                if (topIndex != -1)
                {
                    string trimmed = lines[topIndex].Trim();
                    if (IsPageNumberLine(trimmed) || headersToRemove.Contains(trimmed))
                    {
                        lines.RemoveAt(topIndex);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Strip bottom page numbers and footers
            for (int k = 0; k < 3; k++)
            {
                int lastIndex = GetLastLineIndex(lines);
                if (lastIndex != -1)
                {
                    string trimmed = lines[lastIndex].Trim();
                    if (IsPageNumberLine(trimmed) || footersToRemove.Contains(trimmed))
                    {
                        lines.RemoveAt(lastIndex);
                    }
                    else
                    {
                        break;
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

    public static string DisjoinGluedHeadersAndNumbers(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // 1. Split trailing numbers attached to punctuation/quotes/letters: "saçlar,11" -> "saçlar,\n11", "git.”12" -> "git.”\n12"
        string result = Regex.Replace(text, @"([\.\,\”""'\?\!\w\u00A0-\u024F])(\d{1,4})\b", "$1\n$2");

        // 2. Split uppercase headers attached to lowercase body words: "KADER KILICIsolgun" -> "KADER KILICI\nsolgun"
        result = Regex.Replace(result, @"\b([A-ZÇĞİÖŞÜ]{3,}(?:\s+[A-ZÇĞİÖŞÜ]{2,})*)([a-zçğıöşüA-ZÇĞİÖŞÜ][a-zçğıöşü]{2,})", "$1\n$2");

        // 3. Split author names attached to body words: "Andrzej SapkovvskiSivilceli" -> "Andrzej Sapkovvski\nSivilceli"
        result = Regex.Replace(result, @"\b([A-Z][a-zçğıöşü]+\s+[A-Z][a-zçğıöşü]+)([A-Z][a-zçğıöşü]{2,})", "$1\n$2");

        return result;
    }

    public static bool IsPageNumberLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        string trimmed = line.Trim();

        // 1. Pure positive integers: "16", "172"
        if (int.TryParse(trimmed, out int num) && num > 0 && num < 10000)
            return true;

        // 2. Numbers wrapped in dashes/brackets/spaces: "- 16 -", "— 16 —", "[16]", "(16)"
        string stripped = trimmed.Trim('-', '—', '–', '[', ']', '(', ')', ' ', '.');
        if (int.TryParse(stripped, out int num2) && num2 > 0 && num2 < 10000)
            return true;

        // 3. Keywords like "Sayfa 16", "Page 16", "16 / 350"
        if (Regex.IsMatch(trimmed, @"^(page|sayfa)\s+\d+$", RegexOptions.IgnoreCase))
            return true;

        if (Regex.IsMatch(trimmed, @"^\d+\s*\/\s*\d+$"))
            return true;

        return false;
    }

    private static List<(int index, string text)> GetTopNonEmptyLines(List<string> lines, int count)
    {
        var result = new List<(int index, string text)>();
        for (int i = 0; i < lines.Count && result.Count < count; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                result.Add((i, lines[i]));
            }
        }
        return result;
    }

    private static List<(int index, string text)> GetBottomNonEmptyLines(List<string> lines, int count)
    {
        var result = new List<(int index, string text)>();
        for (int i = lines.Count - 1; i >= 0 && result.Count < count; i--)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                result.Add((i, lines[i]));
            }
        }
        return result;
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
