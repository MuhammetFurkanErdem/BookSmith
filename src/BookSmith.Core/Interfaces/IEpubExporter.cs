using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

public interface IEpubExporter
{
    void Export(EpubExportOptions options);
}
