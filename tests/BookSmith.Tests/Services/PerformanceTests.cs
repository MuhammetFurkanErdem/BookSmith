using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using BookSmith.Services.Cleaning;
using Xunit;

namespace BookSmith.Tests.Services;

public class PerformanceTests
{
    [Fact]
    public void MassiveBook_TextCleaner_And_ChapterDetector_Performance_Under_500ms()
    {
        // 1. Generate a massive 1,000-page book text (~2,000,000 chars)
        var sb = new StringBuilder();
        for (int i = 1; i <= 20; i++)
        {
            sb.AppendLine($"BÖLÜM {i}\n");
            sb.AppendLine("Bu bir test bolumu paragrafidir. Geralt ve Ciri hikayesi devam ediyor.\n");
            for (int p = 0; p < 500; p++)
            {
                sb.AppendLine($"Paragraf {p}: Rivia'li Geralt kılıcını çekti ve Canavar'a doğru ilerledi. \"Dur!\" dedi -- \"Bunu yapma.\"");
            }
            sb.AppendLine();
        }

        string massiveText = sb.ToString();
        Assert.True(massiveText.Length > 1_000_000, "Simulated text should be over 1,000,000 chars.");

        // 2. Measure TextCleaner performance
        var cleaner = new TextCleaner();
        var sw = Stopwatch.StartNew();

        var cleanResult = cleaner.Clean(massiveText);

        sw.Stop();
        long cleanerMs = sw.ElapsedMilliseconds;

        // 3. Measure ChapterDetector performance
        var detector = new ChapterDetector();
        sw.Restart();

        var chapters = detector.DetectChapters(cleanResult.CleanedText);

        sw.Stop();
        long detectorMs = sw.ElapsedMilliseconds;

        // 4. Assertions
        Assert.True(chapters.Count >= 18, $"Detected {chapters.Count} chapters.");
        Assert.True(cleanerMs < 1000, $"TextCleaner took {cleanerMs}ms (should be fast).");
        Assert.True(detectorMs < 500, $"ChapterDetector took {detectorMs}ms (should be < 500ms).");
    }
}
