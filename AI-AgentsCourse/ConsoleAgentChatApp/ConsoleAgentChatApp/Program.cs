using dotenv.net;
using Microsoft.Extensions.Hosting;

DotEnv.Load();

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

