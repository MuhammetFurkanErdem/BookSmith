using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

public interface ISettingsService
{
    AppSettings LoadSettings();
    void SaveSettings(AppSettings settings);
}
