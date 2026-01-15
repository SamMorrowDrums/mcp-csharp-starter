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
 * Every tool SHOULD have annotations to help AI assistants understand behavior:
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

        // Create the bonus tool dynamically from the CalculatorTools.Calculate method
        var calculateMethod = typeof(CalculatorTools).GetMethod(nameof(CalculatorTools.Calculate))!;
        var bonusTool = McpServerTool.Create(calculateMethod);

        // Add to the server's tool collection - this automatically sends
        // the tools/list_changed notification to connected clients
        server.ServerOptions.ToolCollection?.Add(bonusTool);

        _bonusToolLoaded = true;

        return "Bonus tool 'bonus_calculator' has been loaded! The tools list has been updated.";
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

// =============================================================================
// Elicitation Tools - Request user input during tool execution
//
// WHY ELICITATION MATTERS:
// Elicitation allows tools to request additional information from users
// mid-execution, enabling interactive workflows. This is essential for:
//   - Confirming destructive actions before they happen
//   - Gathering missing parameters that weren't provided upfront
//   - Implementing approval workflows for sensitive operations
//   - Collecting feedback or additional context during execution
//
// TWO ELICITATION MODES:
// - Form (schema): Display a structured form with typed fields in the client
// - URL: Open a web page (e.g., OAuth flow, feedback form, documentation)
//
// RESPONSE ACTIONS:
// - "accept": User provided the requested information
// - "decline": User explicitly refused to provide information
// - "cancel": User dismissed the request without responding
// =============================================================================

/// <summary>
/// Elicitation tools demonstrating user input requests
/// </summary>
[McpServerToolType]
public class ElicitationTools
{
    /// <summary>
    /// Demonstrates elicitation - requests user confirmation before proceeding
    /// </summary>
    [McpServerTool(
        Name = "confirm_action",
        Title = "Confirm Action",
        ReadOnly = true,
        Destructive = false,
        Idempotent = false,  // User response varies
        OpenWorld = false,
        IconSource = Icons.Question)]
    [Description("Demonstrates elicitation - requests user confirmation before proceeding")]
    public static async Task<string> ConfirmAction(
        McpServer server,
        [Description("The action to confirm with the user")] string action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if client supports elicitation
            if (server.ClientCapabilities?.Elicitation == null)
            {
                return "Elicitation not supported by this client.";
            }

            // Form elicitation: Display a structured form with typed fields
            // The client renders this as a dialog/form based on the JSON schema
            var result = await server.ElicitAsync(new ElicitRequestParams
            {
                Message = $"Please confirm: {action}",
                RequestedSchema = new ElicitRequestParams.RequestSchema
                {
                    Properties =
                    {
                        ["confirm"] = new ElicitRequestParams.BooleanSchema
                        {
                            Title = "Confirm",
                            Description = "Confirm the action"
                        },
                        ["reason"] = new ElicitRequestParams.StringSchema
                        {
                            Title = "Reason",
                            Description = "Optional reason for your choice"
                        }
                    },
                    Required = ["confirm"]
                }
            }, cancellationToken);

            return result.Action switch
            {
                "accept" when result.Content?.TryGetValue("confirm", out var confirmValue) == true
                    && confirmValue.ValueKind == JsonValueKind.True =>
                    $"Action confirmed: {action}\nReason: {(result.Content.TryGetValue("reason", out var reason) && reason.ValueKind == JsonValueKind.String ? reason.GetString() : "No reason provided")}",
                "accept" => $"Action declined by user: {action}",
                "decline" => $"User declined to respond for: {action}",
                _ => $"User cancelled elicitation for: {action}"
            };
        }
        catch (Exception ex)
        {
            return $"Elicitation not supported or failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Demonstrates URL elicitation - opens a feedback form in the browser
    /// </summary>
    [McpServerTool(
        Name = "get_feedback",
        Title = "Get Feedback",
        ReadOnly = true,
        Destructive = false,
        Idempotent = false,  // User response varies
        OpenWorld = true,    // Opens external URL
        IconSource = Icons.Speech)]
    [Description("Demonstrates URL elicitation - opens a feedback form in the browser")]
    public static async Task<string> GetFeedback(
        McpServer server,
        [Description("Optional topic for the feedback")] string? topic = null,
        CancellationToken cancellationToken = default)
    {
        var feedbackUrl = "https://github.com/SamMorrowDrums/mcp-starters/issues/new?template=workshop-feedback.yml";
        if (!string.IsNullOrEmpty(topic))
        {
            feedbackUrl += $"&title={Uri.EscapeDataString(topic)}";
        }

        try
        {
            // Check if client supports elicitation
            if (server.ClientCapabilities?.Elicitation == null)
            {
                return $"Elicitation not supported by this client.\n\nYou can provide feedback directly at: {feedbackUrl}";
            }

            // URL elicitation: Open a web page in the user's browser
            // Useful for OAuth flows, external forms, documentation links, etc.
            var result = await server.ElicitAsync(new ElicitRequestParams
            {
                Mode = "url",
                ElicitationId = $"feedback-{DateTime.UtcNow.Ticks}",
                Url = feedbackUrl,
                Message = "Please provide feedback on MCP Starters by completing the form at the URL below:"
            }, cancellationToken);

            return result.Action switch
            {
                "accept" => "Thank you for providing feedback! Your input helps improve MCP Starters.",
                "decline" => $"No problem! Feel free to provide feedback anytime at: {feedbackUrl}",
                _ => "Feedback request cancelled."
            };
        }
        catch (Exception ex)
        {
            return $"URL elicitation not supported or failed: {ex.Message}\n\nYou can still provide feedback at: {feedbackUrl}";
        }
    }
}
