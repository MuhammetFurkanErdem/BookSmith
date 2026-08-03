using BookSmith.Services.Cleaning;
using Xunit;

namespace BookSmith.Tests.Cleaning;

public class DialogueFormattingTests
{
    private readonly TextCleaner _textCleaner;

    public DialogueFormattingTests()
    {
        _textCleaner = new TextCleaner();
    }

    [Fact]
    public void FormatDialogueForTts_WithNullOrEmpty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, _textCleaner.FormatDialogueForTts(null!));
        Assert.Equal(string.Empty, _textCleaner.FormatDialogueForTts("   "));
    }

    [Fact]
    public void FormatDialogueForTts_WithGuillemets_NormalizesToCurlyQuotes()
    {
        string input = "«Hello world,» said Alice.";
        string result = _textCleaner.FormatDialogueForTts(input);

        Assert.Equal("“Hello world,” said Alice.", result);
    }

    [Fact]
    public void FormatDialogueForTts_WithHyphenOrEnDash_NormalizesToEmDash()
    {
        string input = "- How are you?\n– I am doing well.\n—Great!";
        string result = _textCleaner.FormatDialogueForTts(input);

        string expected = "— How are you?\n— I am doing well.\n— Great!";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Clean_IncludesDialogueFormattingInPipeline()
    {
        string input = "«Good morning!»\n- Said the teacher.";
        var result = _textCleaner.Clean(input);

        Assert.Contains("“Good morning!”", result.CleanedText);
        Assert.Contains("— Said the teacher.", result.CleanedText);
    }
}
