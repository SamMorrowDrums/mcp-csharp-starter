/*
 * MCP C# Starter - Main Entry Point
 *
 * This file configures and runs the MCP server with either stdio or HTTP transport.
 * 
 * Usage:
 *   dotnet run                    # stdio transport (default)
 *   dotnet run -- --http          # HTTP transport
 *   dotnet run -- --http --port 8080
 *
 * Documentation: https://modelcontextprotocol.io/
 */

using McpCSharpStarter.Tools;
using McpCSharpStarter.Prompts;
using McpCSharpStarter.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

// Server instructions for AI assistants
const string ServerInstructions = """
# MCP C# Starter Server

A demonstration MCP server built with the C# SDK.

## Available Tools

- **hello**: Say hello to a person
- **get_weather**: Get the current weather for a city
- **long_task**: Simulate a long-running task with progress updates
- **ask_llm**: Ask the connected LLM a question using sampling
- **load_bonus_tool**: Dynamically register a new bonus tool
- **confirm_action**: Request user confirmation before proceeding
- **get_feedback**: Request feedback from the user
- **bonus_calculator**: Available after calling load_bonus_tool

## Tool Annotations

Tools include annotations to help AI assistants understand behavior:
- readOnlyHint: Tool only reads data, doesn't modify state
- idempotentHint: Repeated calls with same args have same effect
- openWorldHint: Tool accesses external systems (web, APIs, etc.)

## Available Resources

- **about://server**: Server information
- **doc://example**: Example document
- **greeting://{name}**: Personalized greeting
- **item://{id}**: Item data by ID

## Available Prompts

- **greet**: Generate a greeting message
- **code_review**: Review code for potential improvements
""";

// Shared capabilities configuration for both HTTP and stdio transports
var serverCapabilities = new ServerCapabilities
{
    Experimental = new Dictionary<string, object>(),
    Tools = new ToolsCapability { ListChanged = true },
    Resources = new ResourcesCapability
    {
        ListChanged = false,
        Subscribe = false
    },
    Prompts = new PromptsCapability { ListChanged = false }
};

var useHttp = args.Contains("--http");
var portArg = Array.IndexOf(args, "--port");
var port = portArg >= 0 && portArg + 1 < args.Length ? int.Parse(args[portArg + 1]) : 3000;

if (useHttp)
{
    // HTTP/SSE Transport
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddMcpServer(options =>
        {
            options.ServerInfo = new Implementation
            {
                Name = "mcp-csharp-starter",
                Version = "1.0.0",
                Title = "MCP C# Starter Server",
                Description = "A starter MCP server demonstrating tools, resources, and prompts in C#"
            };
            options.ServerInstructions = ServerInstructions;
            options.Capabilities = serverCapabilities;
        })
        .WithHttpTransport()
        .WithTools<GreetingTools>()
        .WithTools<WeatherTools>()
        .WithTools<SamplingTools>()
        .WithTools<ProgressTools>()
        .WithTools<DynamicTools>()
        .WithTools<ElicitationTools>()
        // Note: CalculatorTools is dynamically loaded via load_bonus_tool
        .WithPrompts<AllPrompts>()
        .WithResources<AllResources>();

    var app = builder.Build();

    app.MapMcp();
    app.MapGet("/health", () => Results.Json(new
    {
        status = "ok",
        server = "mcp-csharp-starter",
        version = "1.0.0"
    }));

    Console.WriteLine($"MCP C# Starter running on http://localhost:{port}");
    Console.WriteLine($"  MCP endpoint: http://localhost:{port}/mcp");
    Console.WriteLine($"  Health check: http://localhost:{port}/health");
    Console.WriteLine("Press Ctrl+C to exit");

    app.Run($"http://localhost:{port}");
}
else
{
    // stdio Transport (default)
    Console.Error.WriteLine("MCP C# Starter running on stdio");
    Console.Error.WriteLine("Press Ctrl+C to exit");

    var builder = Host.CreateApplicationBuilder(args);

    // Disable all logging for stdio transport to avoid interfering with JSON-RPC
    builder.Logging.ClearProviders();

    builder.Services
        .AddMcpServer(options =>
        {
            options.ServerInfo = new Implementation
            {
                Name = "mcp-csharp-starter",
                Version = "1.0.0",
                Title = "MCP C# Starter Server",
                Description = "A starter MCP server demonstrating tools, resources, and prompts in C#"
            };
            options.ServerInstructions = ServerInstructions;
            options.Capabilities = serverCapabilities;
        })
        .WithStdioServerTransport()
        .WithTools<GreetingTools>()
        .WithTools<WeatherTools>()
        .WithTools<SamplingTools>()
        .WithTools<ProgressTools>()
        .WithTools<DynamicTools>()
        .WithTools<ElicitationTools>()
        // Note: CalculatorTools is dynamically loaded via load_bonus_tool
        .WithPrompts<AllPrompts>()
        .WithResources<AllResources>();

    var app = builder.Build();
    await app.RunAsync();
}
