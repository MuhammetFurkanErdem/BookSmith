using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

/// <summary>
/// Interface for LLM AI-powered contextual text reconstruction and connection validation.
/// </summary>
public interface IAiReconstructionService
{
    /// <summary>
    /// Sends <paramref name="text"/> to the configured LLM model to contextually repair OCR errors,
    /// remove inline author headers/footers, and restore natural sentence flow.
    /// </summary>
    Task<string> ReconstructParagraphAsync(
        string text,
        AiModelConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>Tests whether the configured LLM model endpoint is reachable.</summary>
    Task<bool> TestConnectionAsync(AiModelConfig config, CancellationToken cancellationToken = default);
}
