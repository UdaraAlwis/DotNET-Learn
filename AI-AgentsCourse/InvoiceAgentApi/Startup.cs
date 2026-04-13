using Anthropic.SDK;
using GeminiDotnet.Extensions.AI;
using InvoiceAgentApi.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace InvoiceAgentApi
{
    public static class Startup
    {
        public static void ConfigureServices(WebApplicationBuilder builder, string provider, string model)
        {
            var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
            var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")!;
            var claudiaApiKey = Environment.GetEnvironmentVariable("CLAUDIA_API_KEY")!;

            builder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));
            builder.Services.AddSingleton<ILoggerFactory>(sp => LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information)));
        
            builder.Services.AddSingleton<IChatClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>();
                
                var client = provider switch
                {
                    "openai" => new OpenAI.Chat.ChatClient(
                        string.IsNullOrWhiteSpace(model) ? "gpt-4.1-mini" : model,
                        openAiApiKey).AsIChatClient(),

                    "gemini" => new GeminiChatClient(new GeminiDotnet.GeminiClientOptions
                    {
                        ApiKey = geminiApiKey,
                        ModelId =  model,
                    }),

                    "claude" => new AnthropicClient(new APIAuthentication(
                                Environment.GetEnvironmentVariable("CLAUDE_API_KEY")!)).Messages,

                    _ => throw new ArgumentException($"Provider '{provider}' is not supported.")

                };

                return new ChatClientBuilder(client)
                    .UseLogging(logger)
                    .UseFunctionInvocation(logger, c =>
                    {
                        c.IncludeDetailedErrors = true;
                    })
                    .Build(sp);
            });

            builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
            {
                Tools = [.. FunctionRegistry.GetTools(sp)],
                ModelId = model,
                Temperature = 1,
                MaxOutputTokens = 5000
            });

            builder.Services.AddTransient<InvoiceApiClient>();
        }
    }
}
