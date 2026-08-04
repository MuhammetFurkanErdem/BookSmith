using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Cleaning;

public class TurkishSpellChecker : ISpellChecker
{
    private static readonly HashSet<string> TurkishDictionary = new(StringComparer.OrdinalIgnoreCase)
    {
        // General Common Turkish Vocabulary
        "bir", "bu", "şu", "o", "ve", "ile", "de", "da", "ki", "ne", "mı", "mi", "mu", "mü",
        "için", "gibi", "kadar", "daha", "çok", "her", "tüm", "bütün", "var", "yok", "ama", "fakat",
        "ancak", "çünkü", "veya", "yahut", "sonra", "önce", "kendi", "biz", "siz", "onlar", "ben", "sen",
        "hikaye", "cin", "peşinde", "kadın", "büyücü", "fena", "takım", "sayılmaz", "işin", "kötü",
        "bite", "sonu", "bilir", "nedir", "diye", "sordu", "derdi", "büyücüler", "dedi", "güçlerini",
        "doğadaki", "güçlerden", "doğrusu", "dört", "temel", "elementten", "prensipten", "alırlar",
        "bunlar", "hava", "toprak", "ateştir", "elementlerden", "birinin", "kendi", "boyutu", "vardır",
        "jargonunda", "buna", "alem", "denir", "su", "alemi", "ateşin", "bizlerin", "ulaşamadığı",
        "boyutlarda", "dediğimiz", "canlılar", "yaşar", "efsanelerde", "sözünü", "kesti", "bildiğim",
        "kadarıyla", "yanlışın", "söze", "karıştı", "tapı", "nak", "okulu", "değil", "burası", "bize",
        "konferans", "şunu", "söyle", "kısaca", "verme", "istiyor", "öyle", "valim", "sihirli",
        "enerjilerin", "canlı", "yumağıdır", "elinde", "bulunduran", "bu", "enerjiyi", "büyüye",
        "dönüştürüp", "gerekli", "alanlarda", "kullanabilir", "gücünü", "uğraşıp", "doğadan",
        "almasına", "gerek", "kalmaz", "işi", "adına", "böylece", "onun", "yapar", "kudreti",
        "olur", "mutlak", "yaklaşır", "muazzam", "gücü", "vatan", "duymadım", "geldi", "gitti",
        "bak", "baktı", "dedim", "söyledi", "etti", "oldu", "olacak", "olup", "bitti", "gece", "gündüz",
        "zaman", "insan", "gün", "yıl", "ev", "yol", "el", "göz", "baş", "yüz", "ses", "şey",

        // Character Names / Proper Nouns
        "Geralt", "Yennefer", "Witcher", "Neville", "Krepp", "Ciri", "Dandelion", "Vengerberg"
    };

    private static readonly Regex WordSplitRegex = new(@"\b[a-zA-ZçşğüöıÇŞĞÜÖİ]+\b", RegexOptions.Compiled);

    public bool IsWordValid(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return true;

        string cleanWord = word.Trim();

        // Single character words (except a, e, o, vb) or numbers are valid
        if (cleanWord.Length <= 1 || cleanWord.All(char.IsDigit)) return true;

        // Standard dictionary lookup
        if (TurkishDictionary.Contains(cleanWord)) return true;

        // Heuristic: check if word has valid Turkish vowel structure
        if (IsTurkishStructureValid(cleanWord)) return true;

        return false;
    }

    public IReadOnlyList<MisspelledWord> FindMisspelledWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<MisspelledWord>();

        var results = new List<MisspelledWord>();
        var matches = WordSplitRegex.Matches(text);

        foreach (Match match in matches)
        {
            string word = match.Value;

            // Skip numbers or very short words
            if (word.Length <= 2 || word.All(char.IsDigit))
                continue;

            if (!IsWordValid(word))
            {
                results.Add(new MisspelledWord
                {
                    Word = word,
                    StartIndex = match.Index,
                    Suggestions = GetSuggestions(word)
                });
            }
        }

        return results.AsReadOnly();
    }

    public IReadOnlyList<string> GetSuggestions(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return Array.Empty<string>();

        string cleanWord = NormalizeDiacritics(word.Trim().ToLowerInvariant());

        // Find dictionary words with edit distance <= 2 after diacritic normalization
        var candidates = TurkishDictionary
            .Select(w => new { Word = w, Normalized = NormalizeDiacritics(w.ToLowerInvariant()) })
            .Where(x => Math.Abs(x.Normalized.Length - cleanWord.Length) <= 2)
            .Select(x => new { x.Word, Distance = LevenshteinDistance(cleanWord, x.Normalized) })
            .Where(x => x.Distance <= 2)
            .OrderBy(x => x.Distance)
            .Take(3)
            .Select(x => x.Word)
            .ToList();

        return candidates.AsReadOnly();
    }

    private static string NormalizeDiacritics(string text)
    {
        return text.Replace('ç', 'c')
                   .Replace('ğ', 'g')
                   .Replace('ı', 'i')
                   .Replace('ö', 'o')
                   .Replace('ş', 's')
                   .Replace('ü', 'u');
    }

    private static bool IsTurkishStructureValid(string word)
    {
        // OCR corruption detection: 4 or more consecutive consonants (e.g. "kflzmesm")
        int consecutiveConsonants = 0;
        string vowels = "aeıioöuüAEIİOÖUÜ";

        foreach (char c in word)
        {
            if (char.IsLetter(c) && !vowels.Contains(c))
            {
                consecutiveConsonants++;
                if (consecutiveConsonants >= 4)
                    return false; // Likely OCR noise
            }
            else
            {
                consecutiveConsonants = 0;
            }
        }

        return true;
    }

    private static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
}
