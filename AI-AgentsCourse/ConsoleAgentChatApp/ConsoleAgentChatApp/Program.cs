using ConsoleAgentChatApp;
using dotenv.net;
using Microsoft.Extensions.Hosting;

// Load .env file from the correct location
var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
DotEnv.Load(new DotEnvOptions().WithEnvFiles(envPath));

string provider = "openai";
string model = "gpt-4.1-mini";

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--provider" && i+1 < args.Length)
        provider = args[i+1].ToLower();

    if (args[i] == "--model" && i + 1 < args.Length)
        model = args[i + 1].ToLower();
}


var builder = Host.CreateApplicationBuilder(args);
Startup.ConfigureServices(builder, provider, model);
var host = builder.Build();

await ChatAgent.RunAsync(host.Services);