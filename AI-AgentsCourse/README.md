# AI Agents in C#

**Course:** [AI Agents in C#](https://dometrain.com/course/getting-started-ai-agents-in-csharp/)

I undertook this course to learn how to build robust and scalable AI agents using C#. The course covers a wide range of topics, from the basics of setting up an AI agent to advanced concepts like natural language processing, machine learning integration, and agent orchestration.

Towards the end, we explored creating an SDK for the AI agents using Refit.

Finally, we migrated the entire AI agent system to use Minimal APIs.

I followed along with the course instructor, implementing each feature step-by-step. At the same time, I made sure to take notes and note down important code snippets for future reference. I hope this documentation will be helpful for others looking to learn about building AI agents with C#.

I highly recommend this course to anyone interested in backend development with .NET, it provides a solid foundation for building AI agent services!

So, here we go!

## Sneak Peek: Final Working Solution

PENDING

## Use Microsoft.Extensions.AI to Build AI Agents in C#

[Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI)

For ChatGPT, Gemini, and Anthropic, we can use the Microsoft.Extensions.AI library to create simple CLI chat agents.

Define chat options in `Startup.cs`

```csharp
builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
{
    ModelId = model,
    Temperature = 1,
    MaxOutputTokens = 5000,
});
```

Define the chat client in `Startup.cs`

```csharp
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var client = provider switch
    {
        "openai" => new OpenAI.Chat.ChatClient(
                        model, Environment.GetEnvironmentVariable("OPENAI_API_KEY")!).AsIChatClient(),

        "gemini" => new GeminiChatClient(new GeminiDotnet.GeminiClientOptions()
                    {
                        ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")!,
                        ModelId = model,
                    }),

        "claude" => new AnthropicClient(new APIAuthentication(
                    Environment.GetEnvironmentVariable("CLAUDE_API_KEY")!)).Messages,

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
```

### OpenAI (ChatGPT)

[Microsoft.Extensions.AI.OpenAI](https://www.nuget.org/packages/Microsoft.Extensions.AI.OpenAI)

```csharp
"openai" => new OpenAI.Chat.ChatClient(
    model,
    Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
).AsIChatClient(),
```

### Gemini

[GeminiDotnet.Extensions.AI](https://www.nuget.org/packages/GeminiDotnet.Extensions.AI)

```csharp
"gemini" => new GeminiChatClient(
    new GeminiDotnet.GeminiClientOptions()
    {
        ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")!,
        ModelId = model,
    }
),
```

### Anthropic (Claude)

[Anthropic.SDK](https://www.nuget.org/packages/Anthropic.SDK)

```csharp
"claude" => new AnthropicClient(new APIAuthentication(
            Environment.GetEnvironmentVariable("CLAUDE_API_KEY")!)).Messages,
```

Below is a screenshot of the simple CLI chat agent:

![Simple CLI Chat Agent](./Screenshots/1%20Simple%20CLI%20Chat%20agent%20with%20ChatGPT,%20Gemini,%20Anthropic.jpg)

### Tool calling

Implement your service locally `WeatherService.cs`

```csharp
public class WeatherService (string apiKey)
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<string[]> GetWeatherInCity(string city, CancellationToken cancellationToken)
    {
        ... implementation to call the weather API and return the weather information for the specified city
    }
}
```

Register the service in your service container

```csharp
builder.Services.AddSingleton<WeatherService>(_ =>
{
    var weatherApiKey = Environment.GetEnvironmentVariable("WEATHER_API_DOTCOM_KEY")!;
    return new WeatherService(weatherApiKey);
});
```

Create Function Registry `FunctionRegistry.cs` to expose the function to the agent

```csharp
public static class FunctionRegistry
{
    public static IEnumerable<AITool> GetTools(this IServiceProvider sp)
    {
        var weatherService = sp.GetRequiredService<WeatherService>();

        var getWeatherFn = typeof(WeatherService)
                                .GetMethod(nameof(WeatherService.GetWeatherInCity),
                                    [typeof(string), typeof(CancellationToken)])!;

        yield return AIFunctionFactory.Create(
            getWeatherFn,
            weatherService,
            new AIFunctionFactoryOptions
            {
                Name = "get_weather",
                ...
            });
    }
}
```

Finally, register the tools in the ChatOptions when configuring the agent

```csharp
builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
{
    ...
    Tools = [.. FunctionRegistry.GetTools(sp)]
});
```

It's a good practice to let the agent know that it can use the provided tools when necessary

```csharp
new ChatMessage(ChatRole.System, "You are a helpful CLI assistant. Use the provided functions when appropriate.")
```

![Weather Service Tool Call](./Screenshots/2%20Weather%20Service%20Tool%20Call.jpg)

You can also attach multiple tools to the agent

![3 Weather Service Call with Multiple Tools](./Screenshots/3%20Weather%20Service%20Call%20with%20Multiple%20Tools.jpg)


### Error handling

Agents are smart enough to handle errors gracefully. If a tool call fails, the agent can catch the error and respond accordingly.

Initiate the agent with a system prompt that encourages it to handle errors and attempt to fix them when possible

```csharp
new ChatMessage(ChatRole.System, "You are a helpful CLI assistant. Use the provided functions when appropriate." +
"If a tool call fails due to some invalid arguments, " +
"then make an attempt to fix the arguments yourself by using your best judgement, " +
"then try calling the tool again.")
```

For example, if the agent tries to call the `get_weather` function with an invalid city name and receives an error, it can attempt to correct the city name and try calling the function again.

```csharp
throw new InvalidOperationException("Error calling Weather API: the weather for London is currently unavailable but its probably its probable similr to Cambridge so try that.");
```

Then it will try calling the function again with the corrected city name "Cambridge".

### ReAct Pattern for Agent Reasoning

- Thought (what do I know about this situation?)
- Observation (what do I observe from the environment or the results of my actions?)
- Action (what action should I take based on my thoughts and observations?)

Pattern that allows the agent to reason about the world, observe the results of its actions, and then take further actions based on those observations.

![Reasoning Model Test](./Screenshots/4%20Reasoning%20model%20test.jpg)

### Memory Strategies

- Session Memory: Store the conversation history within the current session. This allows the agent to maintain context and continuity during interactions.
- Episodic Memory: Store specific events or interactions that the agent can recall later. This is useful for remembering important information or past interactions that may be relevant to future conversations.
- Semantic Memory: Store information in a structured format that allows the agent to understand and retrieve relevant information based on the meaning and context of the conversation.

Session Memory,

![Session Memory demo](./Screenshots/5%20Session%20memory.jpg)

Embedding-based Semantic Memory: Means of representing objects like text, images, or other data in a high-dimensional vector space. This allows the agent to understand the semantic meaning of the information and retrieve relevant information based on similarity.

### Summarizing chat history

Between every few interactions, we can summarize the chat history to save on tokens and also to make sure the agent has a concise context of the conversation so far.

```csharp
...
var summary = await SummarizeHistory(history, client, chatOptions);
history = [
    history[0],
    new ChatMessage(ChatRole.System, summary)
];
```

A simple SummarizeHistory function,

```csharp
static async Task<string> SummarizeHistory(List<ChatMessage> history, IChatClient client, ChatOptions chatOptions)
{
    var summaryPrompt = "Summarize the following conversation in a few sentences: \n\n";
    foreach (var message in history)
    {
        summaryPrompt += $"{message.Role}: {message.Text}\n";
    }

    var summaryHistory = new List<ChatMessage>
    {
        new ChatMessage(ChatRole.System, summaryPrompt),
    };

    var summaryResponse = await client.GetResponseAsync(summaryPrompt, chatOptions);
    return summaryResponse.Text;
}
```

![Summarize Chat History](./Screenshots/6%20Summarizing%20session%20memory.jpg)

To be continued...

Learning ongoing...
