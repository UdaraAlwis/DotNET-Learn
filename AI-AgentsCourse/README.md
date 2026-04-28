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


### MCP - Model Context Protocol

MCP is a protocol that allows agents to share information and context with each other. 

This enables complex interactions and collaborations between agents such as sharing knowledge and capabilities to achieve their goals.

Core building blocks of MCP include:
- Tools - capabilities that an agent can use to perform specific actions or retrieve information.
- Resources - information or data that an agent can access or manipulate.
- Prompts - instructions or messages that guide the agent's behavior and decision-making process, such as starter prompts, system prompts, or user prompts.

Transports - the means by which agents communicate and exchange information with each other, such as HTTP, WebSockets, or message queues.

- stdio - local communication via standard input/output streams.
- Streamable HTTP - HTTP transport with data streaming support.

MCP Server in .NET,

Create a simple Console app with `ModelContextProtocol` package and `ModelContextProtocol.AspNetCore` package for creating the MCP server.

Add MCP server to the service container

```csharp
builder.Services
    .AddMcpServer()           // Register MCP server
    .WithHttpTransport()      // Enable HTTP transport for communication
    .WithToolsFromAssembly(); // Auto-register tools from the assembly

...

app.MapMcp(); // Map MCP endpoints to the app
```

Bring in the `InvoiceApiClient` and register it as a tool resource in the MCP server

Create your MCP tools with it,

```csharp
[McpServerToolType]
public static class McpTools
{
    [McpServerTool, Description("Retrieves a list of all invoices in the InvoiceApp")]
    public static Task<List<Invoice>> ListInvoices(InvoiceApiClient client)
    {
        return client.ListInvoices();
    }

    ...
}
```

![MCP Server Demo](./Screenshots/11%20Simple%20MCP%20Server%20for%20InvoiceApp%20Agent.jpg)

Install ModelContextProtocol.Inspector tool to inspect and debug the MCP server and the agents connected to it.

[ModelContextProtocol.Inspector](https://github.com/modelcontextprotocol/inspector)

In Windows step by step guide to install and run the inspector

```powershell
PS node -v
PS git --version
PS git clone https://github.com/modelcontextprotocol/inspector.git
PS cd inspector
PS npm install
PS npm run
PS npm run dev
```

This will run the inspector and connect to your MCP server using Streamable HTTP transport.

![MCP Inspector connected to the MCP Server](./Screenshots/12%20MCP%20Inspector%20connected%20to%20my%20MCP%20Server.jpg)

![MCP Inspector listing the tools available](./Screenshots/13%20MCP%20Inspector%20List%20Tools.jpg)

![MCP Inspector calling the tools available through MCP Server](./Screenshots/14%20MCP%20Inspector%20List%20Tool%20calling%20through%20MCP%20Server.jpg)

```
PS npm start
```

### MCP User Prompts

User prompts guide what actions an agent can perform with the MCP server.

### Adding User Prompts to MCP Server

So now in our MCP Server, we're gonna expose default available User Prompts to the clients (any other service or LLM that connects).

In `Program.cs` add the `WithPromptsFromAssembly()` method:

```csharp
builder.Services
    ...
    .WithPromptsFromAssembly();
```

Create a new `McpPrompts` class and use the `[McpServerPromptType]` attribute:

```csharp
[McpServerPromptType]
public static class McpPrompts
{
    [McpServerPrompt, Description("Creates a prompt to pay an invoice")]
    public static ChatMessage PayInvoicePrompt(
        [Description("The name of the invoice to mark as paid")] string invoiceName)
        => new ChatMessage(ChatRole.User, $"Find the invoice \"{invoiceName}\" and mark it as paid.");
}
```

![MCP inspector calling the user prompts available](./Screenshots/15%20MCP%20Server%20User%20Prompts.jpg)

### MCP Resources

Provide context and reusable information that agents can reference when executing tasks. ex: Documentation, DB Schema

### Adding Resources to MCP Server

Our MCP server already has documentation, we're gonna expose that to the clients (any other service or LLM that connects).

In `Program.cs` add the `WithResourcesFromAssembly()` method:

```csharp
builder.Services
    ...
    .WithResourcesFromAssembly();
```

Create a new `McpResources` class and use the `[McpServerResourceType]` attribute:

```csharp
[McpServerResourceType]
public static class McpResources
{
    [McpServerResource(MimeType = "text/markdown"), Description("Document describing how to use the InvoiceApp platform")]
    public static string GetDocumentationMarkdown()
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Docs", "getting-started.md"));
    }
}
```

![MCP inspector calling the Resources available](./Screenshots/16%20MCP%20Server%20Resources.jpg)


### Using our MCP Server from a MCP Client (any LLM Client)

Deploy your MCP Server to the web with an HTTPS certificate and use it in any MCP Client (any LLM).

#### Exposing localhost to the Internet

For this course, we'll use [localhost.run](https://localhost.run/) to forward our localhost port to the internet. This service exposes a process on localhost to the internet and provides a temporary URL with a valid HTTPS certificate.

**Steps:**

1. Ensure your MCP server is running on localhost (e.g., port 5050)

2. Run the following command to expose it:
   ```bash
   ssh -R 80:localhost:5050 -o StrictHostKeyChecking=no nokey@localhost.run
   ```

3. localhost.run will provide you with a public URL in the format:
   ```
   https://<RANDOMALPHANUMERIC>.lhr.life
   ```

4. Use this URL in your MCP Client configuration

![Expose local MCP Server to internet](./Screenshots/17%20Expose%20local%20MCP%20Server%20to%20internet.jpg)

Add the generated https://<RANDOMALPHANUMERIC>.lhr.life local MCP Server linked service to the choice of your LLM or any MCP Client (ex: Claud.ai)

![Add the MCP server to a LLM Client](./Screenshots/18%20Add%20the%20MCP%20server%20to%20a%20LLM%20Client.jpg)

Interact with the MCP Server such as "What can it do?",

![Call the MCP Server through the LLM Client](./Screenshots/19%20Call%20the%20MCP%20Server%20through%20the%20LLM%20Client.jpg)

Invoke tools of the MCP server such as "How many invoices do I have?",

![Call the tools of the MCP Server through the LLM Client](./Screenshots/20%20Call%20the%20tools%20of%20the%20MCP%20Server%20through%20the%20LLM%20Client.jpg)

---

That's the end of the AI Agents in C# course!

![Course Completion Certificate](Screenshots/21%20certificate-qRW1lO8RcMNrHa.png)

Cheers!
