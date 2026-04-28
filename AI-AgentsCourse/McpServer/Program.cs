using McpServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    // Set up MCP Tools
    .WithToolsFromAssembly()
    // Set up MCP Prompts
    .WithPromptsFromAssembly()
    // Set up MCP Resources
    .WithResourcesFromAssembly();

builder.WebHost.UseUrls("http://localhost:5050");

builder.Services.AddSingleton<InvoiceApiClient>();

var app = builder.Build();

app.MapMcp();

app.Run();