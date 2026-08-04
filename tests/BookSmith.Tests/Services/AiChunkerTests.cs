using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;
using BookSmith.Services.Cleaning;
using Xunit;

namespace BookSmith.Tests.Services;

public class AiChunkerTests
{
    private class DummyAiService : IAiReconstructionService
    {
        public Task<string> ReconstructParagraphAsync(string text, AiModelConfig config, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("Geralt diş izleri vardı. Sonra her şey çok hızlı gelişti.");
        }

        public Task<bool> TestConnectionAsync(AiModelConfig config, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    [Fact]
    public void GarbageDensityDetector_DetectsReplacementCharacterNoise()
    {
        string corruptedText = "diş izleri v\uFFFD\uFFFDrdı. kflzmesmeyonususturur";
        double ratio = GarbageDensityDetector.CalculateNoiseRatio(corruptedText);

        Assert.True(ratio > 0.05);
        Assert.True(GarbageDensityDetector.IsCorrupted(corruptedText));
    }

    [Fact]
    public async Task AiTextChunker_RoutesCorruptedParagraphsToAiService()
    {
        var dummyAi = new DummyAiService();
        var chunker = new AiTextChunker(dummyAi);

        string text = "Normal paragraf burada duruyor.\n\n" +
                     "Geralt diş izleri v\uFFFD\uFFFDrdı. ANDRZEJ SAPKOWSKI Sonra her şey çok hızlı gelişti.";

        var config = new AiModelConfig();
        string restored = await chunker.ReconstructFullTextAsync(text, config);

        Assert.Contains("Geralt diş izleri vardı.", restored);
    }
}
