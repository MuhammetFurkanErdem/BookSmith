using System.Collections.Generic;
using BookSmith.Services.Cleaning;
using Xunit;

namespace BookSmith.Tests.Cleaning;

public class HeaderFooterAndPageNumberTests
{
    private readonly TextCleaner _textCleaner;

    public HeaderFooterAndPageNumberTests()
    {
        _textCleaner = new TextCleaner();
    }

    [Theory]
    [InlineData("16", true)]
    [InlineData("172", true)]
    [InlineData("- 16 -", true)]
    [InlineData("— 17 —", true)]
    [InlineData("Sayfa 18", true)]
    [InlineData("Page 19", true)]
    [InlineData("20 / 350", true)]
    [InlineData("Normal text line.", false)]
    [InlineData("Geralt başını kaldırdı.", false)]
    public void IsPageNumberLine_IdentifiesPageNumbersCorrectly(string line, bool expected)
    {
        bool actual = TextCleaner.IsPageNumberLine(line);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RemoveHeadersAndFooters_StripsAlternatingHeadersAndChangingPageNumbers()
    {
        var pages = new List<string>
        {
            "SON DİLEK\n\nAdam yere serilmişti.\n\n16",
            "ANDRZEJ SAPKOWSKI\n\nMuhafızlar derhal geri çekilip yüzlerini örttüler.\n\n17",
            "SON DİLEK\n\nYabancı duvarın önüne çekildi.\n\n18",
            "ANDRZEJ SAPKOWSKI\n\nKendi isteğimle geleceğim dedi.\n\n19"
        };

        string result = _textCleaner.RemoveHeadersAndFooters(pages);

        Assert.DoesNotContain("SON DİLEK", result);
        Assert.DoesNotContain("ANDRZEJ SAPKOWSKI", result);
        Assert.DoesNotContain("16", result);
        Assert.DoesNotContain("17", result);
        Assert.DoesNotContain("18", result);
        Assert.DoesNotContain("19", result);

        Assert.Contains("Adam yere serilmişti.", result);
        Assert.Contains("Muhafızlar derhal geri çekilip yüzlerini örttüler.", result);
    }
}
