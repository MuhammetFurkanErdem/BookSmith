using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Cleaning;

public class BasicTextCleaner : ITextCleaner
{
    public TextCleaningResult Clean(string input)
    {
        if (input == null)
        {
            return new TextCleaningResult
            {
                CleanedText = string.Empty,
                IsModified = false
            };
        }

        string trimmed = input.Trim();
        return new TextCleaningResult
        {
            CleanedText = trimmed,
            IsModified = trimmed != input
        };
    }
}
