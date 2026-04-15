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

### Caching on the provider

Prompt caching caches provider responses to reduce latency and improve performance for frequently used prompts.

OpenAi, Anthropic, and Gemini providers all support caching, explicit and automatic options.


### InvoiceApp Chat Agent

- Create an InvoiceAgentApi in Visual Studio with .NET 8

- Create a system prompt that defines the agent's behavior and capabilities. Make sure to read it and initialize the agent with it when it starts up.

SystemPrompt.txt

```
You are an agent that controls an invoicing platform called 'InvoiceApp' and also provides assistance on how to use the platform.

The platform stores invoices in various states (Pending, Paid etc) and can perform actions directly on the platform such as

* Creating a new invoice
* Searching for an invoice by name
* Marking an invoice as paid

General guidance

* Give the user general advice on managing invoices
* You can read their invoices using the available tools
* When writing messages back keep them short and to the point
```

```csharp
var systemPromptPath = Path.Combine(AppContext.BaseDirectory, "SystemPrompt.txt");
var systemPrompt = File.ReadAllText(systemPromptPath);
```

- Create endpoint for client app

```csharp
app.MapPost("/chat", async (
    List<ChatMessage> messages,
    IChatClient client,
    ChatOptions chatOptions) =>
{
    var systemPromptWithDate = systemPrompt + "\n By the way, today's date is " + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".";

    var withSystemPrompt = (new [] { new ChatMessage(ChatRole.System, systemPromptWithDate) })
                                .Concat(messages).ToList();

    var response = await client.GetResponseAsync(withSystemPrompt, chatOptions);
    return Results.Ok(response.Messages);
});
```

- In Startup.cs, configure the chat client and options 

```csharp
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var logger = sp.GetRequiredService<ILoggerFactory>();
    
    var client = provider switch
    {
        ...
    };

    return new ChatClientBuilder(client)
        .UseLogging(logger)
        .UseFunctionInvocation(logger, c =>
        {
            c.IncludeDetailedErrors = true;
        })
        .Build(sp);
});
```

```csharp
builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
{
    Tools = [.. FunctionRegistry.GetTools(sp)],
    ...
});
```

- Create a `InvoiceApiClient` that calls the Invoice API DB and exposes functions like `ListInvoices`, `FindInvoiceByName`, etc.

- Register that in the `FunctionRegistry` to expose those functions to the agent

```csharp
yield return AIFunctionFactory.Create(
    typeof(InvoiceApiClient).GetMethod(nameof(InvoiceApiClient.ListInvoices), 
    ...
    new AIFunctionFactoryOptions
    {
        Name = "list_invoices",
        ...
    });

yield return AIFunctionFactory.Create(
    typeof(InvoiceApiClient).GetMethod(nameof(InvoiceApiClient.FindInvoiceByName),
    [typeof(string)])!,
    apiClient,
    new AIFunctionFactoryOptions
    {
        Name = "find_invoice_by_name",
        ...
    });
```

![InvoiceApp Chat Agent](./Screenshots/7%20InvoiceApp%20Chat%20agent.jpg)

### RAG - Retrieval Augmented Generation (RAG) 

This is a technique that combines retrieval-based methods with generative models to improve the quality and relevance of generated responses. 
In a RAG system, the agent retrieves relevant information from a knowledge base or external sources and then uses that information to generate more accurate and contextually appropriate responses.

### Knowledge Bases (Simpler alternative to vector databases)

Put the .md files into a folder and allow the agent to read and retrieve information from those files when needed. 
This is a simpler alternative to setting up a vector database for retrieval.

Create the "Docs" folder with .md files

Then create a 'DocumentationClient' that reads the .md files and exposes a function to retrieve relevant information based on a query.

```csharp
public class DocumentationClient
{
    private readonly string _docsDirectory;

    public DocumentationClient()
    {
        _docsDirectory = Path.Combine(AppContext.BaseDirectory, "Docs");
    }

    public string? GetDocumentationPage(string pageName)
    {
        ...

        return File.ReadAllText(filePath);
    }
}
```

Register that as a tool in the `FunctionRegistry`

```csharp
var docService = sp.GetRequiredService<DocumentationClient>();

yield return AIFunctionFactory.Create(typeof(DocumentationClient).GetMethod(nameof(DocumentationClient.GetDocumentationPage),
    [typeof(string)])!,
    docService,
    new AIFunctionFactoryOptions
    {
        Name = "read_documentation_page",
        Description = "Retrieves the contents of this page in the documentation"
    });
```

Update the `SystemPrompt.txt` to let the agent know it can use the documentation client to read the docs when needed

```
...
If you need to provide assistance to the user to help them use the UI themselves,
There are four pages of documentation for this platform.
You can retrieve any page by using the read_documentation_page tool

* "getting-started" - a basic overview of the platform and how to use it
* "viewing-invoices" - how to view invoices on the platform"
* "creating-invoices" - how t create invoices on the platform
* "managing-invoices" - how to manage invoices on the platform
...
```

![Knowledge Base Demo](./Screenshots/8%20InvoiceApp%20Chat%20agent%20with%20Knowledge%20base.jpg)

Adding create and update status functionalities to the agent,

```csharp
public async Task<Invoice> CreateInvoice(CreateInvoiceRequest request)
{
    var newInvoice = new Invoice
    {
        Description = request.Description,
        Amount = request.Amount,
        Due = request.DueDate ?? DateTime.UtcNow.AddDays(30),
        Date = DateTime.UtcNow,
        Status = "Pending"
    };

    ... make the API call to create the invoice and return the created invoice
}
```     

```csharp
public async Task MarkAsPaid(int invoiceId)
{
    var request = new UpdateInvoiceRequest
    {
        Status = "Paid"
    };

    ... make the API call to update the invoice status
}
```

Register those in the `FunctionRegistry` as well

![Create and Update Invoice Demo](./Screenshots/9%20InvoiceApp%20Chat%20agent%20with%20actionable%20functionality.jpg)

![Create and Update Invoice Demo](./Screenshots/10%20InvoiceApp%20Chat%20agent%20with%20actionable%20functionality.jpg)

---

**To be continued...**

*Learning ongoing...*
