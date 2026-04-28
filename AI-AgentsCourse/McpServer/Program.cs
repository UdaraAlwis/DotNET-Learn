using McpServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

builder.WebHost.UseUrls("http://localhost:5050");

builder.Services.AddSingleton<InvoiceApiClient>();

var app = builder.Build();

app.MapMcp();

app.Run();