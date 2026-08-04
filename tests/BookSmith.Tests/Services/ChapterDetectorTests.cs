using System.Linq;
using BookSmith.Services.Cleaning;
using Xunit;

namespace BookSmith.Tests.Services;

public class ChapterDetectorTests
{
    private ChapterDetector CreateDetector() => new ChapterDetector();

    [Fact]
    public void DetectChapters_Detects_Turkish_Chapters()
    {
        var detector = CreateDetector();
        string text = "BÖLÜM 1\nHikaye burada basliyor.\n\nBölüm 2\nIkinci bolum metni devam ediyor.";

        var result = detector.DetectChapters(text);

        Assert.Equal(2, result.Count);
        Assert.Equal("BÖLÜM 1", result[0].Title);
        Assert.Equal("Bölüm 2", result[1].Title);
    }

    [Fact]
    public void DetectChapters_Detects_English_Chapters()
    {
        var detector = CreateDetector();
        string text = "CHAPTER 1\nThe story begins.\n\nChapter IV\nThe adventure continues.";

        var result = detector.DetectChapters(text);

        Assert.Equal(2, result.Count);
        Assert.Equal("CHAPTER 1", result[0].Title);
        Assert.Equal("Chapter IV", result[1].Title);
    }

    [Fact]
    public void DetectChapters_Detects_Roman_Numerals()
    {
        var detector = CreateDetector();
        string text = "II\nFirst real section.\n\nXIV\nLater section.";

        var result = detector.DetectChapters(text);

        Assert.Equal(2, result.Count);
        Assert.Equal("II", result[0].Title);
        Assert.Equal("XIV", result[1].Title);
    }

    [Fact]
    public void DetectChapters_Ignores_Single_I_Pronoun()
    {
        var detector = CreateDetector();
        string text = "I\nSome text.\n\nI went to the store and I bought a book.";

        var result = detector.DetectChapters(text);

        // Standalone 'I' is guarded against in ChapterDetector
        Assert.Empty(result);
    }

    [Fact]
    public void DetectChapters_Detects_Ordinal_Numbers()
    {
        var detector = CreateDetector();
        string text = "1.\nFirst chapter text.\n\n2.\nSecond chapter text.";

        var result = detector.DetectChapters(text);

        Assert.Equal(2, result.Count);
        Assert.Equal("1.", result[0].Title);
        Assert.Equal("2.", result[1].Title);
    }

    [Fact]
    public void DetectChapters_Preserves_Correct_Offsets_And_Indexes()
    {
        var detector = CreateDetector();
        string text = "BÖLÜM 1\nLine 1\n\nBÖLÜM 2\nLine 2";

        var result = detector.DetectChapters(text);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Index);
        Assert.Equal(0, result[0].CharOffset);

        Assert.Equal(2, result[1].Index);
        Assert.True(result[1].CharOffset > result[0].CharOffset);
    }

    [Fact]
    public void DetectChapters_Returns_Empty_For_Null_Or_Empty_Input()
    {
        var detector = CreateDetector();

        Assert.Empty(detector.DetectChapters(null!));
        Assert.Empty(detector.DetectChapters(""));
        Assert.Empty(detector.DetectChapters("   "));
    }
}
