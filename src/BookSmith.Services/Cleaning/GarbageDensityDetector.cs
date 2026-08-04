using System;
using System.Linq;

namespace BookSmith.Services.Cleaning;

/// <summary>
/// Analyzes text or paragraph chunks to calculate an OCR noise/garbage character ratio (0.0 to 1.0).
/// </summary>
public static class GarbageDensityDetector
{
    public static double CalculateNoiseRatio(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0.0;

        int totalChars = text.Length;
        int noiseCount = 0;

        int consecutiveConsonants = 0;
        string vowels = "aeıioöuüAEIİOÖUÜ";

        for (int i = 0; i < totalChars; i++)
        {
            char c = text[i];

            // 1. Unicode replacement character or non-printable controls
            if (c == '\uFFFD' || char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
            {
                noiseCount += 3; // Heavily weight replacement characters
                continue;
            }

            // 2. Track excessive consecutive consonants (OCR spacing collapse)
            if (char.IsLetter(c) && !vowels.Contains(c))
            {
                consecutiveConsonants++;
                if (consecutiveConsonants >= 4)
                {
                    noiseCount++;
                }
            }
            else
            {
                consecutiveConsonants = 0;
            }

            // 3. Question mark inside word bounds (e.g. "v?rdı")
            if (c == '?' && i > 0 && i < totalChars - 1 && char.IsLetter(text[i - 1]) && char.IsLetter(text[i + 1]))
            {
                noiseCount += 2;
            }
        }

        return Math.Min(1.0, (double)noiseCount / totalChars);
    }

    public static bool IsCorrupted(string text, double threshold = 0.05)
    {
        return CalculateNoiseRatio(text) >= threshold;
    }
}
