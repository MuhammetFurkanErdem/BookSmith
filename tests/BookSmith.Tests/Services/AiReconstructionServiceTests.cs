using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BookSmith.Core.Models;
using BookSmith.Services.Cleaning;
using Xunit;

namespace BookSmith.Tests.Services;

public class AiReconstructionServiceTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseJson;

        public MockHttpMessageHandler(string responseJson)
        {
            _responseJson = responseJson;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task ReconstructParagraphAsync_ParsesOllamaResponseSuccessfully()
    {
        string mockJson = "{\"response\":\"Geralt diş izleri vardı. Sonra her şey çok hızlı gelişti.\"}";
        using var client = new HttpClient(new MockHttpMessageHandler(mockJson));
        var service = new OllamaAiService(client);

        var config = new AiModelConfig { EndpointUrl = "http://localhost:11434/api/generate", ModelName = "llama3" };
        string input = "Geralt diş izleri vrdı. ANDRZEJ SAPKOWSKI Sonra her şey çok hızlı gelişti.";

        string result = await service.ReconstructParagraphAsync(input, config);

        Assert.Equal("Geralt diş izleri vardı. Sonra her şey çok hızlı gelişti.", result);
    }

    [Fact]
    public async Task TestConnectionAsync_ReturnsTrueForSuccessfulEndpoint()
    {
        string mockJson = "{\"response\":\"Ping OK\"}";
        using var client = new HttpClient(new MockHttpMessageHandler(mockJson));
        var service = new OllamaAiService(client);

        var config = new AiModelConfig();
        bool isConnected = await service.TestConnectionAsync(config);

        Assert.True(isConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ReturnsFalseOnNetworkException()
    {
        var service = new OllamaAiService();
        var invalidConfig = new AiModelConfig { EndpointUrl = "http://invalid-localhost-url-99999/api/generate" };

        bool isConnected = await service.TestConnectionAsync(invalidConfig);

        Assert.False(isConnected);
    }
}
