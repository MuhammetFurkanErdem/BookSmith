using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Cleaning;

/// <summary>
/// Splits full book text into manageable paragraphs/chunks and routes corrupted sections
/// through the IAiReconstructionService for LLM restoration.
/// </summary>
public class AiTextChunker
{
    private readonly IAiReconstructionService _aiService;

    public AiTextChunker(IAiReconstructionService aiService)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
    }

    public async Task<string> ReconstructFullTextAsync(
        string text,
        AiModelConfig config,
        Action<int, int, string>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text) || config == null)
            return text ?? string.Empty;

        // Split text by paragraphs
        string[] paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        int total = paragraphs.Length;
        var resultSb = new StringBuilder();

        for (int i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string current = paragraphs[i].Trim();
            progressCallback?.Invoke(i + 1, total, $"AI processing paragraph {i + 1}/{total}...");

            // If paragraph is corrupted or contains inline header artifacts, send to AI
            if (GarbageDensityDetector.IsCorrupted(current) || ContainsStrayHeaderArtifacts(current))
            {
                try
                {
                    string restored = await _aiService.ReconstructParagraphAsync(current, config, cancellationToken);
                    resultSb.AppendLine(restored);
                    resultSb.AppendLine();
                }
                catch
                {
                    // Fallback to original paragraph on AI service failure
                    resultSb.AppendLine(current);
                    resultSb.AppendLine();
                }
            }
            else
            {
                // Clean paragraph, pass through directly
                resultSb.AppendLine(current);
                resultSb.AppendLine();
            }
        }

        return resultSb.ToString().TrimEnd();
    }

    private static bool ContainsStrayHeaderArtifacts(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        // Check for uppercase inline author / book header patterns like "ANDRZEJ SAPKOWSKI"
        return text.Contains("SAPKOWSKI", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("WITCHER", StringComparison.OrdinalIgnoreCase);
    }
}
