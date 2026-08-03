using System;
using System.IO;
using BookSmith.Services.Pdf;
using Xunit;

namespace BookSmith.Tests.Pdf;

public class PdfPigReaderTests
{
    private readonly PdfPigReader _reader;

    public PdfPigReaderTests()
    {
        _reader = new PdfPigReader();
    }

    [Fact]
    public void ReadAllPages_WithNullFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _reader.ReadAllPages(null!));
    }

    [Fact]
    public void ReadAllPages_WithEmptyFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _reader.ReadAllPages(string.Empty));
    }

    [Fact]
    public void ReadAllPages_WithWhitespaceFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _reader.ReadAllPages("   "));
    }

    [Fact]
    public void ReadAllPages_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
        Assert.Throws<FileNotFoundException>(() => _reader.ReadAllPages(nonExistentPath));
    }

    [Fact]
    public void ReadMetadata_WithNullFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _reader.ReadMetadata(null!));
    }

    [Fact]
    public void ReadFirstPageText_WithNullFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _reader.ReadFirstPageText(null!));
    }
}
