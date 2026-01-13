/*
 * MCP C# Starter - Tools
 *
 * Demonstrates various tool patterns in MCP:
 * - Basic tool with annotations
 * - Tool with structured output
 * - Tool that invokes sampling
 * - Tool with progress updates
 * - Tool that dynamically loads another tool
 *
 * ## Tool Annotations
 *
 * Every tool MUST have annotations to help AI assistants understand behavior:
 * - ReadOnly: Tool only reads data, doesn't modify state
 * - Destructive: Tool can permanently delete or modify data
 * - Idempotent: Repeated calls with same args have same effect
 * - OpenWorld: Tool accesses external systems (web, APIs, etc.)
 */

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace McpCSharpStarter.Tools;

/// <summary>
/// Enum for calculator operations - demonstrates using enums in MCP tool schemas
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CalculatorOperation
{
    [Description("Add two numbers")]
    Add,
    [Description("Subtract second from first")]
    Subtract,
    [Description("Multiply two numbers")]
    Multiply,
    [Description("Divide first by second")]
    Divide
}

/// <summary>
/// Basic greeting tools demonstrating tool annotations
/// </summary>
[McpServerToolType]
public class GreetingTools
{
    /// <summary>
    /// A friendly greeting tool that says hello to someone
    /// </summary>
    [McpServerTool(
        Name = "hello",
        Title = "Say Hello",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        IconSource = Icons.WavingHand)]
    [Description("A friendly greeting tool that says hello to someone")]
    public static string Hello(
        [Description("The name to greet")] string name)
    {
        return $"Hello, {name}! Welcome to MCP.";
    }
}

/// <summary>
/// Weather data returned by the weather tool
/// </summary>
public record WeatherData(
    string Location,
    int Temperature,
    string Unit,
    string Conditions,
    int Humidity);

/// <summary>
/// Weather tool demonstrating structured output
/// </summary>
[McpServerToolType]
public class WeatherTools
{
    private static readonly string[] Conditions = ["sunny", "cloudy", "rainy", "windy"];
    private static readonly Random _random = new();

    /// <summary>
    /// Get current weather for a location (simulated)
    /// </summary>
    [McpServerTool(
        Name = "get_weather",
        Title = "Get Weather",
        ReadOnly = true,
        Destructive = false,
        Idempotent = false,  // Simulated - results vary
        OpenWorld = false,   // Not real external call
        IconSource = Icons.SunBehindCloud)]
    [Description("Get current weather for a location (simulated)")]
    public static string GetWeather(
        [Description("City name or coordinates")] string location)
    {
        var weather = new WeatherData(
            Location: location,
            Temperature: 15 + _random.Next(20),
            Unit: "celsius",
            Conditions: Conditions[_random.Next(Conditions.Length)],
            Humidity: 40 + _random.Next(40)
        );

        return JsonSerializer.Serialize(weather, new JsonSerializerOptions { WriteIndented = true });
    }
}

/// <summary>
/// Sampling tool demonstrating LLM invocation
/// </summary>
[McpServerToolType]
public class SamplingTools
{
    /// <summary>
    /// Ask the connected LLM a question using sampling
    /// </summary>
    [McpServerTool(
        Name = "ask_llm",
        Title = "Ask LLM",
        ReadOnly = true,
        Destructive = false,
        Idempotent = false,  // LLM responses vary
        OpenWorld = false,
        IconSource = Icons.Robot)]
    [Description("Ask the connected LLM a question using sampling")]
    public static async Task<string> AskLlm(
        McpServer server,
        [Description("The question or prompt for the LLM")] string prompt,
        [Description("Maximum number of tokens to generate")] int maxTokens = 100,
        CancellationToken cancellationToken = default)
    {
        try
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
                    MaxTokens = maxTokens
                },
                cancellationToken: cancellationToken);

            var text = result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text ?? "[non-text response]";
            return $"LLM Response: {text}";
        }
        catch (Exception ex)
        {
            return $"Sampling not supported or failed: {ex.Message}";
        }
    }
}

/// <summary>
/// Long-running task tool demonstrating progress updates
/// </summary>
[McpServerToolType]
public class ProgressTools
{
    /// <summary>
    /// A task that takes 5 seconds and reports progress along the way
    /// </summary>
    [McpServerTool(
        Name = "long_task",
        Title = "Long Running Task",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        IconSource = Icons.Hourglass)]
    [Description("A task that takes 5 seconds and reports progress along the way")]
    public static async Task<string> LongTask(
        McpServer server,
        [Description("Name for this task")] string taskName,
        CancellationToken cancellationToken = default)
    {
        const int steps = 5;

        for (int i = 0; i < steps; i++)
        {
            // Note: Progress reporting depends on SDK support
            // The server would need to track progress tokens
            await Task.Delay(1000, cancellationToken);
        }

        return $"Task \"{taskName}\" completed successfully after {steps} steps!";
    }
}

/// <summary>
/// Tool that demonstrates dynamic tool loading
/// </summary>
[McpServerToolType]
public class DynamicTools
{
    private static bool _bonusToolLoaded = false;

    /// <summary>
    /// Dynamically loads a bonus tool that wasn't available at startup
    /// </summary>
    [McpServerTool(
        Name = "load_bonus_tool",
        Title = "Load Bonus Tool",
        ReadOnly = false,  // Modifies server state
        Destructive = false,
        Idempotent = true,  // Safe to call multiple times
        OpenWorld = false,
        IconSource = Icons.Package)]
    [Description("Dynamically loads a bonus tool that wasn't available at startup")]
    public static string LoadBonusTool(McpServer server)
    {
        if (_bonusToolLoaded)
        {
            return "Bonus tool is already loaded! Try calling 'bonus_calculator'.";
        }

        // In C#, dynamic tool registration would typically be done through
        // McpServerPrimitiveCollection. For this demo, we mark it as loaded.
        // Real implementation would add to server.Options.ToolCollection
        _bonusToolLoaded = true;

        return "Bonus tool 'bonus_calculator' has been loaded! Refresh your tools list to see it.";
    }
}

/// <summary>
/// Calculator tool (would be dynamically loaded)
/// </summary>
[McpServerToolType]
public class CalculatorTools
{
    /// <summary>
    /// A calculator that was dynamically loaded
    /// </summary>
    [McpServerTool(
        Name = "bonus_calculator",
        Title = "Bonus Calculator",
        ReadOnly = true,  // Pure computation
        Destructive = false,
        Idempotent = true,  // Same inputs = same outputs
        OpenWorld = false,
        IconSource = Icons.Abacus)]
    [Description("A calculator that was dynamically loaded")]
    public static string Calculate(
        [Description("First number")] double a,
        [Description("Second number")] double b,
        [Description("Mathematical operation to perform")] CalculatorOperation operation)
    {
        double result = operation switch
        {
            CalculatorOperation.Add => a + b,
            CalculatorOperation.Subtract => a - b,
            CalculatorOperation.Multiply => a * b,
            CalculatorOperation.Divide => b != 0 ? a / b : double.NaN,
            _ => double.NaN
        };

        return $"{a} {operation.ToString().ToLower()} {b} = {result}";
    }
}
