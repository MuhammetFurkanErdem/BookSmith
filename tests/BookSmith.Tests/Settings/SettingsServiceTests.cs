using System;
using System.IO;
using BookSmith.Core.Models;
using BookSmith.Infrastructure.Settings;
using Xunit;

namespace BookSmith.Tests.Settings;

public class SettingsServiceTests
{
    [Fact]
    public void LoadSettings_WhenFileDoesNotExist_ReturnsDefaultSettings()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        var service = new JsonSettingsService(tempPath);

        var settings = service.LoadSettings();

        Assert.NotNull(settings);
        Assert.True(settings.RemoveHeaders);
        Assert.True(settings.RemoveFooters);
        Assert.False(settings.ElevenReaderMode);
    }

    [Fact]
    public void SaveSettings_AndLoadSettings_PersistsAllProperties()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        try
        {
            var service = new JsonSettingsService(tempPath);

            var customSettings = new AppSettings
            {
                RemoveHeaders = false,
                RemoveFooters = true,
                RemovePageNumbers = false,
                FixBrokenWords = true,
                MergeWrappedLines = false,
                SmartDialogueFormatting = true,
                ElevenReaderMode = true,
                ExportEpub = true,
                DefaultLanguage = "en"
            };

            service.SaveSettings(customSettings);

            Assert.True(File.Exists(tempPath));

            var loaded = service.LoadSettings();
            Assert.NotNull(loaded);
            Assert.False(loaded.RemoveHeaders);
            Assert.True(loaded.RemoveFooters);
            Assert.False(loaded.RemovePageNumbers);
            Assert.True(loaded.FixBrokenWords);
            Assert.False(loaded.MergeWrappedLines);
            Assert.True(loaded.SmartDialogueFormatting);
            Assert.True(loaded.ElevenReaderMode);
            Assert.True(loaded.ExportEpub);
            Assert.Equal("en", loaded.DefaultLanguage);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
