using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using BookSmith.Core.Models;
using BookSmith.Services.Export;
using Xunit;

namespace BookSmith.Tests.Services;

public class EpubExporterTests : IDisposable
{
    private readonly string _tempDirectory;

    public EpubExporterTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "BookSmithTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try { Directory.Delete(_tempDirectory, true); } catch { }
        }
    }

    private EpubExporter CreateExporter() => new EpubExporter();

    [Fact]
    public void Export_WithoutCover_CreatesValidEpubZip()
    {
        var exporter = CreateExporter();
        string outputPath = Path.Combine(_tempDirectory, "test_no_cover.epub");

        var options = new EpubExportOptions
        {
            Title = "Test Title",
            Author = "Test Author",
            OutputPath = outputPath,
            ContentText = "Chapter 1\n\nThis is a test paragraph."
        };

        exporter.Export(options);

        Assert.True(File.Exists(outputPath));

        using var archive = ZipFile.OpenRead(outputPath);

        Assert.NotNull(archive.GetEntry("mimetype"));
        Assert.NotNull(archive.GetEntry("META-INF/container.xml"));
        Assert.NotNull(archive.GetEntry("OEBPS/content.opf"));
        Assert.NotNull(archive.GetEntry("OEBPS/toc.ncx"));
        Assert.NotNull(archive.GetEntry("OEBPS/style.css"));
        Assert.NotNull(archive.GetEntry("OEBPS/chapter1.xhtml"));
        Assert.Null(archive.GetEntry("OEBPS/cover.jpg"));
    }

    [Fact]
    public void Export_WithCover_PackagesCoverImageAndCoverXhtml()
    {
        var exporter = CreateExporter();
        string outputPath = Path.Combine(_tempDirectory, "test_with_cover.epub");
        string coverPath = Path.Combine(_tempDirectory, "fake_cover.jpg");

        // Create a dummy image file
        File.WriteAllBytes(coverPath, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });

        var options = new EpubExportOptions
        {
            Title = "Cover Test Title",
            Author = "Cover Test Author",
            OutputPath = outputPath,
            ContentText = "Content with cover",
            CoverImagePath = coverPath
        };

        exporter.Export(options);

        Assert.True(File.Exists(outputPath));

        using var archive = ZipFile.OpenRead(outputPath);

        var coverImageEntry = archive.GetEntry("OEBPS/cover.jpg");
        var coverPageEntry = archive.GetEntry("OEBPS/cover.xhtml");

        Assert.NotNull(coverImageEntry);
        Assert.NotNull(coverPageEntry);

        // Verify content.opf references the cover
        var opfEntry = archive.GetEntry("OEBPS/content.opf");
        Assert.NotNull(opfEntry);

        using var reader = new StreamReader(opfEntry.Open(), Encoding.UTF8);
        string opfText = reader.ReadToEnd();

        Assert.Contains("meta name=\"cover\" content=\"cover-image\"", opfText);
        Assert.Contains("id=\"cover-image\"", opfText);
        Assert.Contains("id=\"cover\"", opfText);
    }

    [Fact]
    public void Export_WithCustomTypography_AppliesStyleCss()
    {
        var exporter = CreateExporter();
        string outputPath = Path.Combine(_tempDirectory, "test_style.epub");

        var options = new EpubExportOptions
        {
            Title = "Styled Book",
            OutputPath = outputPath,
            ContentText = "Styled content text.",
            FontSizePt = 16,
            LineHeight = 1.8
        };

        exporter.Export(options);

        using var archive = ZipFile.OpenRead(outputPath);
        var cssEntry = archive.GetEntry("OEBPS/style.css");
        Assert.NotNull(cssEntry);

        using var reader = new StreamReader(cssEntry.Open(), Encoding.UTF8);
        string cssText = reader.ReadToEnd();

        Assert.Contains("font-size: 16pt;", cssText);
        Assert.Contains("line-height: 1.8;", cssText);
    }
}
