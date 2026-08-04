using System;
using System.IO;
using System.Linq;
using BookSmith.Core.Models;
using BookSmith.Infrastructure.Settings;
using Xunit;

namespace BookSmith.Tests.Services;

public class PresetManagerTests : IDisposable
{
    private readonly string _tempFilePath;

    public PresetManagerTests()
    {
        string dir = Path.Combine(Path.GetTempPath(), "BookSmithPresetTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        _tempFilePath = Path.Combine(dir, "presets.json");
    }

    public void Dispose()
    {
        try
        {
            string? dir = Path.GetDirectoryName(_tempFilePath);
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
        catch { }
    }

    private JsonPresetManager CreateManager() => new JsonPresetManager(_tempFilePath);

    [Fact]
    public void GetAllPresets_ReturnsBuiltInPresetsByDefault()
    {
        var manager = CreateManager();
        var presets = manager.GetAllPresets();

        Assert.NotEmpty(presets);
        Assert.Contains(presets, p => p.Name == "Default Rules" && p.IsBuiltIn);
        Assert.Contains(presets, p => p.Name == "ElevenReader Fiction" && p.IsBuiltIn);
    }

    [Fact]
    public void SavePreset_AddsCustomPreset()
    {
        var manager = CreateManager();

        var custom = new CleaningPreset
        {
            Name = "My Custom Preset",
            Description = "Test description",
            IsBuiltIn = false,
            RemoveHeaders = false,
            ElevenReaderMode = true
        };

        manager.SavePreset(custom);

        var presets = manager.GetAllPresets();
        Assert.Contains(presets, p => p.Name == "My Custom Preset" && !p.IsBuiltIn);
    }

    [Fact]
    public void DeletePreset_RemovesCustomPreset()
    {
        var manager = CreateManager();

        var custom = new CleaningPreset
        {
            Name = "ToDelete Preset",
            IsBuiltIn = false
        };

        manager.SavePreset(custom);
        Assert.NotNull(manager.GetPresetById(custom.Id));

        bool deleted = manager.DeletePreset(custom.Id);

        Assert.True(deleted);
        Assert.Null(manager.GetPresetById(custom.Id));
    }

    [Fact]
    public void DeletePreset_CannotDeleteBuiltInPreset()
    {
        var manager = CreateManager();
        var builtIn = manager.GetAllPresets().First(p => p.IsBuiltIn);

        bool deleted = manager.DeletePreset(builtIn.Id);

        Assert.False(deleted);
        Assert.NotNull(manager.GetPresetById(builtIn.Id));
    }
}
