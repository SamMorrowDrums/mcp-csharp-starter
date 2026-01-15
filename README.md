# MCP C# Starter

[![CI](https://github.com/SamMorrowDrums/mcp-csharp-starter/actions/workflows/ci.yml/badge.svg)](https://github.com/SamMorrowDrums/mcp-csharp-starter/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![MCP](https://img.shields.io/badge/MCP-Model%20Context%20Protocol-purple)](https://modelcontextprotocol.io/)

A feature-complete Model Context Protocol (MCP) server template in C# using the official csharp-sdk. This starter demonstrates all major MCP features with clean, idiomatic C# code leveraging .NET 8 and dependency injection.

## 📚 Documentation

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Building MCP Servers](https://modelcontextprotocol.io/docs/develop/build-server)

## ✨ Features

| Category | Feature | Description |
|----------|---------|-------------|
| **Tools** | `hello` | Basic tool with annotations |
| | `get_weather` | Tool returning structured JSON |
| | `ask_llm` | Tool that invokes LLM sampling |
| | `long_task` | Tool with progress updates |
| | `load_bonus_tool` | Dynamically loads a new tool |
| | `bonus_calculator` | Calculator (dynamically loaded) |
| **Resources** | `info://about` | Static informational resource |
| | `file://example.md` | File-based markdown resource |
| **Templates** | `greeting://{name}` | Personalized greeting |
| | `data://items/{id}` | Data lookup by ID |
| **Prompts** | `greet` | Greeting in various styles |
| | `code_review` | Code review with focus areas |

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Installation

```bash
# Clone the repository
git clone https://github.com/SamMorrowDrums/mcp-csharp-starter.git
cd mcp-csharp-starter

# Restore packages
dotnet restore
```

### Running the Server

**stdio transport** (for local development):
```bash
dotnet run
```

**HTTP transport** (for remote/web deployment):
```bash
dotnet run -- --http
# Or with custom port:
dotnet run -- --http --port 8080
# Server runs on http://localhost:3000 by default
```

## 🔧 VS Code Integration

This project includes VS Code configuration for seamless development:

1. Open the project in VS Code
2. The MCP configuration is in `.vscode/mcp.json`
3. Build with `Ctrl+Shift+B` (or `Cmd+Shift+B` on Mac)
4. Debug with F5 (configurations for both transports)
5. Test the server using VS Code's MCP tools

### Using DevContainers

1. Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
2. Open command palette: "Dev Containers: Reopen in Container"
3. Everything is pre-configured and ready to use!

## 📁 Project Structure

```
.
├── Program.cs             # Main entry point (stdio/HTTP)
├── Tools/
│   └── AllTools.cs        # All tool definitions
├── Resources/
│   └── AllResources.cs    # All resource definitions
├── Prompts/
│   └── AllPrompts.cs      # All prompt definitions
├── .vscode/
│   ├── mcp.json           # MCP server configuration
│   ├── tasks.json         # Build/run tasks
│   ├── launch.json        # Debug configurations
│   └── extensions.json
├── .devcontainer/
│   └── devcontainer.json
├── McpCSharpStarter.csproj
├── global.json
└── appsettings.json
```

## 🛠️ Development

```bash
# Development with live reload (recommended)
dotnet watch run

# Build
dotnet build

# Run tests
dotnet test

# Format code
dotnet format

# Clean
dotnet clean

# Publish for production
dotnet publish -c Release
```

### Live Reload

The `dotnet watch run` command provides automatic rebuilds during development.
Changes to any `.cs` file will automatically rebuild and restart the server.

## 🔍 MCP Inspector

The [MCP Inspector](https://modelcontextprotocol.io/docs/tools/inspector) is an essential development tool for testing and debugging MCP servers.

### Running Inspector

```bash
npx @modelcontextprotocol/inspector -- dotnet run -- --stdio
```

### What Inspector Provides

- **Tools Tab**: List and invoke all registered tools with parameters
- **Resources Tab**: Browse and read resources and templates
- **Prompts Tab**: View and test prompt templates
- **Logs Tab**: See JSON-RPC messages between client and server
- **Schema Validation**: Verify tool input/output schemas

### Debugging Tips

1. Start Inspector before connecting your IDE/client
2. Use the "Logs" tab to see exact request/response payloads
3. Test tool annotations (ReadOnlyHint, etc.) are exposed correctly
4. Verify progress notifications appear for `long_task`
5. Check that McpServer injection works for sampling tools

## 📖 Feature Examples

### Tool with Attributes

```csharp
[McpServerToolType]
public class GreetingTools
{
    [McpServerTool(Name = "hello", Title = "Say Hello")]
    [Description("A friendly greeting tool")]
    public static string Hello(
        [Description("The name to greet")] string name)
    {
        return $"Hello, {name}!";
    }
}
```

### Resource Template

```csharp
[McpServerResourceType]
public class StaticResources
{
    [McpServerResource(
        UriTemplate = "greeting://{name}",
        Name = "Personalized Greeting",
        MimeType = "text/plain")]
    public static string Greeting(string name)
    {
        return $"Hello, {name}!";
    }
}
```

### Tool with Sampling

```csharp
[McpServerTool(Name = "ask_llm")]
public static async Task<string> AskLlm(
    McpServer server,
    [Description("The prompt")] string prompt,
    CancellationToken cancellationToken)
{
    var result = await server.SampleAsync(
        new CreateMessageRequestParams
        {
            Messages = [
                new SamplingMessage
                {
                    Role = Role.User,
                    Content = [new TextContentBlock { Text = prompt }]
                }
            ],
            MaxTokens = 100
        },
        cancellationToken: cancellationToken);
    
    return result.Content.OfType<TextContentBlock>()
        .FirstOrDefault()?.Text ?? "";
}
```

### Prompt Definition

```csharp
[McpServerPromptType]
public class CodeReviewPrompts
{
    [McpServerPrompt(Name = "code_review", Title = "Code Review")]
    public static IEnumerable<PromptMessage> CodeReview(
        [Description("The code to review")] string code,
        [Description("Programming language")] string language)
    {
        return [
            new PromptMessage
            {
                Role = Role.User,
                Content = new TextContentBlock 
                { 
                    Text = $"Review this {language} code:\n```{language}\n{code}\n```" 
                }
            }
        ];
    }
}
```

## 🔐 Configuration

Configuration via `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## 🤝 Contributing

Contributions welcome! Please ensure your changes maintain feature parity with other language starters.

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.
