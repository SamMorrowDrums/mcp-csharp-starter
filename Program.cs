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

// =============================================================================
// SERVER INSTRUCTIONS
// Instructions are sent to AI assistants when they connect. They describe what
// this server can do, helping the LLM use tools, resources, and prompts
// effectively without the user needing to explain everything.
// =============================================================================
const string ServerInstructions = """
# MCP C# Starter Server

A demonstration MCP server showcasing C# SDK capabilities.

## Recommended Workflows

1. **Test connectivity** → Call `hello` to verify the server responds
2. **Structured output** → Call `get_weather` to see typed response data
3. **Progress reporting** → Call `long_task` to observe real-time progress notifications
4. **Dynamic tools** → Call `load_bonus_tool`, then re-list tools to see `bonus_calculator` appear
5. **LLM sampling** → Call `ask_llm` to have the server request a completion from the client
6. **Elicitation** → Call `confirm_action` (form-based) or `get_feedback` (URL-based) to request user input

## Multi-Tool Flows

- **Full demo**: `hello` → `get_weather` → `long_task` → `load_bonus_tool` → `bonus_calculator`
- **Dynamic loading**: `load_bonus_tool` triggers a `tools/list_changed` notification — refresh your tool list to see `bonus_calculator`
- **User interaction**: `confirm_action` demonstrates schema elicitation, `get_feedback` demonstrates URL elicitation

## Notes

- All tools include annotations (readOnlyHint, idempotentHint, openWorldHint) to guide safe usage
- Resources and prompts are available for context and templating — use `resources/list` and `prompts/list` to discover them
""";

// =============================================================================
// SERVER CAPABILITIES
// Capabilities tell the client what features this server supports. The client
// uses these during initialization to know what it can request.
//
// ListChanged: When true, the server may send notifications that its list of
// tools/resources/prompts has changed at runtime (e.g., after load_bonus_tool
// dynamically adds a new tool). When false, the list is static.
// =============================================================================
var serverCapabilities = new ServerCapabilities
{
    Experimental = new Dictionary<string, object>(),
    // Tools.ListChanged = true because load_bonus_tool adds tools dynamically at runtime.
    // The server sends a tools/list_changed notification so clients refresh their tool list.
    Tools = new ToolsCapability { ListChanged = true },
    // Resources and Prompts are static — they don't change after startup.
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

// =============================================================================
// TRANSPORT SELECTION
// MCP supports multiple transports for communication between client and server:
//
// - stdio: Client launches the server as a subprocess and communicates via
//   stdin/stdout using JSON-RPC. Best for local tools and editor integrations.
//   Logging must be disabled to avoid corrupting the JSON-RPC stream.
//
// - HTTP (Streamable HTTP): Server runs as a web service. Clients connect over
//   HTTP, enabling remote servers and multi-client scenarios. Includes a health
//   endpoint for monitoring.
// =============================================================================
if (useHttp)
{
    // HTTP Transport — server runs as a web service clients connect to over the network
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
    // stdio Transport (default) — client launches this server as a subprocess
    // and communicates via stdin/stdout using JSON-RPC messages
    Console.Error.WriteLine("MCP C# Starter running on stdio");
    Console.Error.WriteLine("Press Ctrl+C to exit");

    var builder = Host.CreateApplicationBuilder(args);

    // IMPORTANT: Disable all logging for stdio — any text written to stdout that
    // isn't valid JSON-RPC will break the protocol. Use stderr for diagnostics.
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
