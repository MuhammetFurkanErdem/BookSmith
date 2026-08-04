namespace BookSmith.Core.Models;

public enum AiProvider
{
    OllamaLocal,
    OpenAiCloud,
    GeminiCloud
}

/// <summary>
/// Configuration for LLM AI model connections (Ollama local, OpenAI, or Gemini Cloud).
/// </summary>
public class AiModelConfig
{
    public AiProvider Provider { get; set; } = AiProvider.OllamaLocal;
    public string EndpointUrl { get; set; } = "http://localhost:11434/api/generate";
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "llama3";
    public double Temperature { get; set; } = 0.2;
}
