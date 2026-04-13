using dotenv.net;
using InvoiceAgentApi;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if(string.IsNullOrWhiteSpace(origin)) return false;

            try
            {
                var uri = new Uri(origin);
                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                          uri.Host.Equals("127.0.0.1");
            }
            catch
            {
                return false;
            }
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5001);
});

// Load .env file from the correct location
var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
DotEnv.Load(new DotEnvOptions().WithEnvFiles(envPath));

string provider = "openai";
string model = "gpt-4.1-mini";

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--provider" && i + 1 < args.Length)
        provider = args[i + 1].ToLower();

    if (args[i] == "--model" && i + 1 < args.Length)
        model = args[i + 1].ToLower();
}

Startup.ConfigureServices(builder, provider, model);

var systemPromptPath = Path.Combine(AppContext.BaseDirectory, "SystemPrompt.txt");
var systemPrompt = File.ReadAllText(systemPromptPath);

var app = builder.Build();

app.UseCors("AllowLocalhost");

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

app.Run();