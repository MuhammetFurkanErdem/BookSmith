using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Cleaning;

public class OllamaAiService : IAiReconstructionService
{
    private readonly HttpClient _httpClient;

    public const string SystemPromptTemplate =
        "You are BookSmith AI, a specialized book editor and text restoration engine for audiobooks.\n" +
        "Your task is to repair corrupted PDF text while strictly preserving story meaning, author intent, and original language flow.\n\n" +
        "Rules:\n" +
        "1. Fix OCR encoding bugs, typos, and broken words (e.g. 'vrdı' -> 'vardı').\n" +
        "2. Remove stray inline author names, book titles, or running header/footer artifacts that break sentence flow (e.g. 'ANDRZEJ SAPKOWSKI').\n" +
        "3. DO NOT change valid story elements, character names, or proper nouns.\n" +
        "4. Output ONLY the clean restored text without markdown wrappers, introduction, or commentary.\n\n" +
        "Text to restore:\n{0}";

    public OllamaAiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<string> ReconstructParagraphAsync(
        string text,
        AiModelConfig config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        if (config == null)
            throw new ArgumentNullException(nameof(config));

        string prompt = string.Format(SystemPromptTemplate, text);

        var payload = new
        {
            model = string.IsNullOrWhiteSpace(config.ModelName) ? "llama3" : config.ModelName,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = config.Temperature > 0 ? config.Temperature : 0.2
            }
        };

        string jsonPayload = JsonSerializer.Serialize(payload);
        using var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        string endpoint = string.IsNullOrWhiteSpace(config.EndpointUrl)
            ? "http://localhost:11434/api/generate"
            : config.EndpointUrl;

        using var response = await _httpClient.PostAsync(endpoint, requestContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(jsonResponse);

        if (doc.RootElement.TryGetProperty("response", out var respElement))
        {
            string restored = respElement.GetString() ?? string.Empty;
            return restored.Trim();
        }

        return text;
    }

    public async Task<bool> TestConnectionAsync(AiModelConfig config, CancellationToken cancellationToken = default)
    {
        if (config == null) return false;

        try
        {
            string endpoint = string.IsNullOrWhiteSpace(config.EndpointUrl)
                ? "http://localhost:11434/api/generate"
                : config.EndpointUrl;

            var payload = new
            {
                model = string.IsNullOrWhiteSpace(config.ModelName) ? "llama3" : config.ModelName,
                prompt = "Ping",
                stream = false
            };

            string jsonPayload = JsonSerializer.Serialize(payload);
            using var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(endpoint, requestContent, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
