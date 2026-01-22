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

A demonstration MCP server showcasing C# SDK capabilities.

## Available Tools

### Greeting & Demos
- **hello**: Simple greeting - use to test connectivity
- **get_weather**: Returns simulated weather data
- **long_task**: Demonstrates progress reporting (takes ~5 seconds)

### LLM Interaction
- **ask_llm**: Invoke LLM sampling to ask questions (requires client support)

### Dynamic Features
- **load_bonus_tool**: Dynamically adds a calculator tool at runtime
- **bonus_calculator**: Available after calling load_bonus_tool

### Elicitation (User Input)
- **confirm_action**: Demonstrates schema elicitation - requests user confirmation
- **get_feedback**: Demonstrates URL elicitation - opens feedback form in browser

## Available Resources

- **about://server**: Server information
- **doc://example**: Sample markdown document
- **greeting://{name}**: Personalized greeting template
- **item://{id}**: Item data by ID

## Available Prompts

- **greeting**: Generates a personalized greeting
- **code_review**: Structured code review prompt

## Recommended Workflows

1. **Testing Connection**: Call `hello` with your name to verify the server is responding
2. **Weather Demo**: Call `get_weather` with a location to see structured output
3. **Progress Demo**: Call `long_task` to see progress notifications
4. **Dynamic Loading**: Call `load_bonus_tool`, then refresh tools to see `bonus_calculator`
5. **Elicitation Demo**: Call `confirm_action` to see user confirmation flow
6. **URL Elicitation**: Call `get_feedback` to open a feedback form

## Tool Annotations

All tools include annotations indicating:
- Whether they modify state (readOnlyHint)
- If they're safe to retry (idempotentHint)
- Whether they access external systems (openWorldHint)

Use these hints to make informed decisions about tool usage.
""";

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
            options.Capabilities = new ServerCapabilities
            {
                Experimental = new Dictionary<string, object>(),
                Tools = new ToolsCapability { ListChanged = true },
                Resources = new ResourcesCapability
                {
                    ListChanged = true,
                    Subscribe = true
                },
                Prompts = new PromptsCapability { ListChanged = true }
            };
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
            options.Capabilities = new ServerCapabilities
            {
                Experimental = new Dictionary<string, object>(),
                Tools = new ToolsCapability { ListChanged = true },
                Resources = new ResourcesCapability
                {
                    ListChanged = true,
                    Subscribe = true
                },
                Prompts = new PromptsCapability { ListChanged = true }
            };
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
