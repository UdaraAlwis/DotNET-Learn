using RawJsonOpenAIChatApp.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RawJsonOpenAIChatApp.Services;

public class OpenAiService
{
    private readonly HttpClient _httpClient = new HttpClient();

    public OpenAiService(string apiKey)
    {
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<ChatMessage> CompleteChat(List<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var requestBody = new ChatRequest
        {
            Model = "gpt-4.1-nano",
            Messages = messages
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody, _jsonOptions);

        using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            if(!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"OpenAI API request failed with status code {response.StatusCode}: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<ChatResponse>(responseContent, _jsonOptions) 
                ?? throw new InvalidOperationException("Failed to deserialize OpenAI API response.");

            var firstChoice = result.Choices?.FirstOrDefault()
                ?? throw new InvalidOperationException("No choices returned from OpenAI API.");

            return new ChatMessage
            {
                Role = firstChoice.Message.Role,
                Content = firstChoice.Message.Content
            };
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Error calling OpenAI API", ex);
        }
    }
}

