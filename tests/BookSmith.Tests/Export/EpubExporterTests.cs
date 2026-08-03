using System;
using System.IO;
using System.IO.Compression;
using BookSmith.Core.Models;
using BookSmith.Services.Export;
using Xunit;

namespace BookSmith.Tests.Export;

public class EpubExporterTests
{
    private readonly EpubExporter _exporter;

    public EpubExporterTests()
    {
        _exporter = new EpubExporter();
    }

    [Fact]
    public void Export_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _exporter.Export(null!));
    }

    [Fact]
    public void Export_WithNullOutputPath_ThrowsArgumentException()
    {
        var options = new EpubExportOptions { OutputPath = null! };
        Assert.Throws<ArgumentException>(() => _exporter.Export(options));
    }

    [Fact]
    public void Export_WithEmptyOutputPath_ThrowsArgumentException()
    {
        var options = new EpubExportOptions { OutputPath = string.Empty };
        Assert.Throws<ArgumentException>(() => _exporter.Export(options));
    }

    [Fact]
    public void Export_WithValidOptions_GeneratesValidEpubArchive()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".epub");

        try
        {
            var options = new EpubExportOptions
            {
                Title = "Test Book",
                Author = "Test Author",
                Language = "en",
                OutputPath = tempPath,
                ContentText = "Paragraph 1 line text.\n\nParagraph 2 line text."
            };

            _exporter.Export(options);

            Assert.True(File.Exists(tempPath));

            using (var archive = ZipFile.OpenRead(tempPath))
            {
                var mimetypeEntry = archive.GetEntry("mimetype");
                var containerEntry = archive.GetEntry("META-INF/container.xml");
                var opfEntry = archive.GetEntry("OEBPS/content.opf");
                var ncxEntry = archive.GetEntry("OEBPS/toc.ncx");
                var cssEntry = archive.GetEntry("OEBPS/style.css");
                var chapterEntry = archive.GetEntry("OEBPS/chapter1.xhtml");

                Assert.NotNull(mimetypeEntry);
                Assert.NotNull(containerEntry);
                Assert.NotNull(opfEntry);
                Assert.NotNull(ncxEntry);
                Assert.NotNull(cssEntry);
                Assert.NotNull(chapterEntry);

                using (var reader = new StreamReader(chapterEntry.Open()))
                {
                    string htmlContent = reader.ReadToEnd();
                    Assert.Contains("Paragraph 1 line text.", htmlContent);
                    Assert.Contains("Paragraph 2 line text.", htmlContent);
                }
            }
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
