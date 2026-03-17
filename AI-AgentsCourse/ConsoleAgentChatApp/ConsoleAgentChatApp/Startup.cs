using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using dotenv.net;

namespace ConsoleAgentChatApp;

public static class Startup
{
    public static void ConfigureServices(HostApplicationBuilder builder, string provider, string model)
    {
        builder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));
        builder.Services.AddSingleton<ILoggerFactory>(sp => 
                                LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information)));

        builder.Services.AddSingleton<IChatClient>(sp => 
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var client = provider switch
            {
                "openai" => new OpenAI.Chat.ChatClient(model, Environment.GetEnvironmentVariable("OPENAI_API_KEY")!).AsIChatClient(),
                
                _ => throw new ArgumentException($"Provider '{provider}' is not supported.")
            };

            return new ChatClientBuilder(client)
                .UseLogging(loggerFactory)
                .UseFunctionInvocation(loggerFactory, c =>
                {
                    c.IncludeDetailedErrors = true;
                })
                .Build();
        });

        builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
        {
            ModelId = model,
            Temperature = 1,
            MaxOutputTokens = 5000
        });
    }
}