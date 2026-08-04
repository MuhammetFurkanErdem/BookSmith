using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Cleaning;

/// <summary>
/// Detects chapter headings in cleaned book text using pattern matching.
/// Supports Turkish (BÖLÜM), English (CHAPTER), Roman numerals, and simple ordinal patterns.
/// Only short standalone lines (≤ 60 chars) are treated as headings.
/// </summary>
public class ChapterDetector : IChapterDetector
{
    private const int MaxHeadingLength = 60;

    // Turkish chapter patterns: BÖLÜM 1, Bölüm 1, BÖLÜM BİR, Bölüm Bir
    private static readonly Regex TurkishPattern = new(
        @"^(BÖLÜM|Bölüm|BOLUM|Bolum)\s+(\d+|[A-ZÇŞĞÜÖİa-zçşğüöı]+)$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    // English chapter patterns: CHAPTER 1, Chapter 1, CHAPTER IV, Chapter One
    private static readonly Regex EnglishPattern = new(
        @"^(CHAPTER|Chapter|chapter)\s+(\d+|[IVXLCDM]+|[A-Za-z]+)$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    // Roman numerals standalone line (I through XCIX, at least 1 character)
    private static readonly Regex RomanPattern = new(
        @"^(?=[MDCLXVI]+$)(M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3}))$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    // Simple ordinal: "1.", "2.", "15." alone on a line (max 5 chars)
    private static readonly Regex OrdinalPattern = new(
        @"^\d{1,3}\.$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    // Generic PART / KISIM / KISIM patterns
    private static readonly Regex PartPattern = new(
        @"^(PART|Part|KISIM|KIŞIM|KISIM|Kısım|BÖLÜM|KİTAP|Kitap)\s+(ONE|TWO|THREE|FOUR|FIVE|SIX|SEVEN|EIGHT|NINE|TEN|\d+|[IVXLCDM]+)$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <inheritdoc/>
    public IReadOnlyList<ChapterInfo> DetectChapters(string cleanedText)
    {
        if (string.IsNullOrWhiteSpace(cleanedText))
            return Array.Empty<ChapterInfo>();

        var chapters = new List<(int Offset, string Title)>();
        var seen = new HashSet<int>(); // prevent duplicate offsets

        void AddMatches(Regex pattern)
        {
            foreach (Match m in pattern.Matches(cleanedText))
            {
                if (m.Length == 0)
                    continue;

                string trimmed = m.Value.Trim();

                // Skip empty or too-long matches
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length > MaxHeadingLength)
                    continue;

                // Roman numeral guard: skip single letter "I" that's just a pronoun
                if (trimmed.Equals("I", StringComparison.Ordinal))
                    continue;

                if (!seen.Contains(m.Index))
                {
                    seen.Add(m.Index);
                    chapters.Add((m.Index, trimmed));
                }
            }
        }

        AddMatches(TurkishPattern);
        AddMatches(EnglishPattern);
        AddMatches(RomanPattern);
        AddMatches(OrdinalPattern);
        AddMatches(PartPattern);

        // Sort by position in text
        chapters.Sort((a, b) => a.Offset.CompareTo(b.Offset));

        var result = new List<ChapterInfo>(chapters.Count);
        for (int i = 0; i < chapters.Count; i++)
        {
            result.Add(new ChapterInfo
            {
                Index = i + 1,
                Title = chapters[i].Title,
                CharOffset = chapters[i].Offset
            });
        }

        return result.AsReadOnly();
    }
}
