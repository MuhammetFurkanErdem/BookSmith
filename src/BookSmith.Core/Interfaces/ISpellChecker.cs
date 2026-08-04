using System.Collections.Generic;
using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

/// <summary>
/// Interface for live spell checking, word validity scoring, and correction suggestions.
/// </summary>
public interface ISpellChecker
{
    /// <summary>Checks whether a single word is valid according to Turkish dictionary & morphology rules.</summary>
    bool IsWordValid(string word);

    /// <summary>Scans text and returns a list of misspelled or corrupted words with offsets.</summary>
    IReadOnlyList<MisspelledWord> FindMisspelledWords(string text);

    /// <summary>Generates correction suggestions for a misspelled word based on edit distance.</summary>
    IReadOnlyList<string> GetSuggestions(string word);
}
