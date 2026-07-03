using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

public interface ITextCleaner
{
    TextCleaningResult Clean(string input);
}
