using System.Linq;
using BookSmith.Services.Cleaning;
using Xunit;

namespace BookSmith.Tests.Services;

public class SpellCheckerTests
{
    private TurkishSpellChecker CreateChecker() => new TurkishSpellChecker();

    [Fact]
    public void IsWordValid_ValidTurkishWords_ReturnsTrue()
    {
        var checker = CreateChecker();

        Assert.True(checker.IsWordValid("hikaye"));
        Assert.True(checker.IsWordValid("büyücü"));
        Assert.True(checker.IsWordValid("Geralt"));
        Assert.True(checker.IsWordValid("Witcher"));
    }

    [Fact]
    public void FindMisspelledWords_DetectsCorruptedOcrNoise_ReturnsMisspelledList()
    {
        var checker = CreateChecker();
        string text = "Güzel bir hikaye. kflzmesmeyonususturur ve bildiğiolkldkm. Geralt büyücü ile konuştu.";

        var misspelled = checker.FindMisspelledWords(text);

        Assert.NotEmpty(misspelled);
        Assert.Contains(misspelled, m => m.Word == "kflzmesmeyonususturur");
    }

    [Fact]
    public void GetSuggestions_GeneratesEditDistanceSuggestions()
    {
        var checker = CreateChecker();

        var suggestions = checker.GetSuggestions("buyucu");

        Assert.Contains("büyücü", suggestions);
    }

    [Fact]
    public void FindMisspelledWords_NullOrEmpty_ReturnsEmpty()
    {
        var checker = CreateChecker();

        Assert.Empty(checker.FindMisspelledWords(null!));
        Assert.Empty(checker.FindMisspelledWords(""));
    }
}
