using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

public interface IBookPipeline
{
    BookProcessingResult Process(string filePath);
}
