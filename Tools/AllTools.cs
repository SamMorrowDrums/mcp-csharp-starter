/*
 * MCP C# Starter - Tools
 *
 * Tools are functions that an AI assistant can invoke on behalf of the user.
 * They are the primary way MCP servers expose actions to clients.
 *
 * HOW IT WORKS IN C#:
 * - [McpServerToolType] marks a class as containing MCP tools
 * - [McpServerTool] marks a method as an MCP tool and sets its annotations
 * - [Description] on parameters generates the tool's input schema automatically
 * - The C# SDK uses reflection to build JSON schemas from method signatures
 *
 * TOOL ANNOTATIONS - Every tool SHOULD have annotations for AI assistants
 *
 * WHY ANNOTATIONS MATTER:
 * Annotations enable MCP client applications to understand the risk level of
 * tool calls. Clients can use these hints to implement safety policies
 * (e.g., auto-approve read-only tools, require confirmation for destructive ones).
 *
 * ANNOTATION FIELDS:
 * - ReadOnly:     Tool only reads data, doesn't modify state
 * - Destructive:  Tool can permanently delete or modify data
 * - Idempotent:   Repeated calls with same args have same effect
 * - OpenWorld:    Tool accesses external systems (web, APIs, etc.)
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
/// Basic greeting tool — the simplest possible MCP tool.
/// Demonstrates: annotations, input schema from method parameters, string return.
/// </summary>
[McpServerToolType]
public class GreetingTools
{
    /// <summary>
    /// Say hello to a person.
    /// ReadOnly + Idempotent: safe to call without side effects or confirmation.
    /// </summary>
    [McpServerTool(
        Name = "hello",
        Title = "Say Hello",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        IconSource = Icons.WavingHand)]
    [Description("Say hello to a person")]
    public static string Hello(
        [Description("Name of the person to greet")] string name)
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
/// Weather tool — demonstrates structured content output.
/// UseStructuredContent = true generates an outputSchema from the return type,
/// so clients know the shape of the response without parsing free text.
/// </summary>
[McpServerToolType]
public class WeatherTools
{
    private static readonly string[] Conditions = ["sunny", "cloudy", "rainy", "windy"];
    private static readonly Random _random = new();

    /// <summary>
    /// Get the current weather for a city
    /// </summary>
    /// <remarks>
    /// This tool demonstrates structured content output using the WeatherData record type.
    /// UseStructuredContent is enabled to generate an outputSchema that describes the
    /// structure of the returned weather data, making it easier for clients to parse and use.
    /// </remarks>
    [McpServerTool(
        Name = "get_weather",
        Title = "Get Weather",
        ReadOnly = true,
        Destructive = false,
        Idempotent = false,  // Simulated - results vary
        OpenWorld = false,   // Not real external call
        UseStructuredContent = true,
        IconSource = Icons.SunBehindCloud)]
    [Description("Get the current weather for a city")]
    public static WeatherData GetWeather(
        [Description("City name to get weather for")] string city)
    {
        return new WeatherData(
            Location: city,
            Temperature: 15 + _random.Next(20),
            Unit: "celsius",
            Conditions: Conditions[_random.Next(Conditions.Length)],
            Humidity: 40 + _random.Next(40)
        );
    }
}

/// <summary>
/// Sampling tool — demonstrates server-initiated LLM calls.
/// MCP "sampling" lets a tool ask the *client's* LLM a question, enabling
/// agentic patterns where tools can reason and make decisions.
/// The McpServer parameter is injected automatically by the SDK.
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
        [Description("The question or prompt to send to the LLM")] string prompt,
        [Description("Maximum tokens in response")] int maxTokens = 100,
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
/// Progress tool — demonstrates long-running tasks.
/// In production, you'd send progress notifications so the client can show
/// a progress bar. The McpServer parameter enables this communication.
/// </summary>
[McpServerToolType]
public class ProgressTools
{
    /// <summary>
    /// Simulate a long-running task with progress updates
    /// </summary>
    [McpServerTool(
        Name = "long_task",
        Title = "Long Running Task",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        IconSource = Icons.Hourglass)]
    [Description("Simulate a long-running task with progress updates")]
    public static async Task<string> LongTask(
        McpServer server,
        [Description("Name for this task")] string taskName,
        [Description("Number of steps to simulate")] int steps = 5,
        CancellationToken cancellationToken = default)
    {
        // Validate steps parameter
        if (steps < 1)
        {
            return "Error: steps must be at least 1";
        }
        if (steps > 100)
        {
            return "Error: steps must not exceed 100 to prevent excessive delays";
        }

        for (int i = 0; i < steps; i++)
        {
            // Note: Progress reporting depends on SDK support
            // The server would need to track progress tokens
            await Task.Delay(1000, cancellationToken);
        }

        return $"Task \"{taskName}\" completed successfully after {steps} steps!";
    }
}

// =============================================================================
// DYNAMIC TOOL LOADING
// This demonstrates adding tools at runtime. When load_bonus_tool is called,
// it registers a new tool (bonus_calculator) and the server notifies clients
// via a tools/list_changed notification (enabled by ListChanged = true in
// capabilities). Clients refresh their tool list in response.
// =============================================================================

/// <summary>
/// Dynamic tool loader — demonstrates runtime tool registration.
/// ReadOnly = false because it modifies server state (the tool list).
/// Idempotent = true because calling it again after loading is a no-op.
/// </summary>
[McpServerToolType]
public class DynamicTools
{
    private static bool _bonusToolLoaded = false;

    /// <summary>
    /// Dynamically register a new bonus tool
    /// </summary>
    [McpServerTool(
        Name = "load_bonus_tool",
        Title = "Load Bonus Tool",
        ReadOnly = false,  // Modifies server state
        Destructive = false,
        Idempotent = true,  // Safe to call multiple times
        OpenWorld = false,
        IconSource = Icons.Package)]
    [Description("Dynamically register a new bonus tool")]
    public static string LoadBonusTool(McpServer server)
    {
        if (_bonusToolLoaded)
        {
            return "Bonus tool is already loaded! Try calling 'bonus_calculator'.";
        }

        // Dynamically create a tool from the CalculatorTools.Calculate method.
        // McpServerTool.Create uses reflection to build the tool from its attributes.
        var calculateMethod = typeof(CalculatorTools).GetMethod(nameof(CalculatorTools.Calculate))!;
        var bonusTool = McpServerTool.Create(calculateMethod);

        // Adding to ToolCollection automatically sends the tools/list_changed
        // notification to connected clients (because ListChanged = true in capabilities)
        server.ServerOptions.ToolCollection?.Add(bonusTool);

        _bonusToolLoaded = true;

        return "Bonus tool 'bonus_calculator' has been loaded! The tools list has been updated.";
    }
}

/// <summary>
/// Calculator tool — loaded dynamically by load_bonus_tool.
/// Not registered at startup (not in .WithTools&lt;CalculatorTools&gt;() in Program.cs).
/// ReadOnly + Idempotent: pure computation, no side effects.
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
    /// Request user confirmation before proceeding
    /// </summary>
    [McpServerTool(
        Name = "confirm_action",
        Title = "Confirm Action",
        ReadOnly = true,
        Destructive = false,
        Idempotent = false,  // User response varies
        OpenWorld = false,
        IconSource = Icons.Question)]
    [Description("Request user confirmation before proceeding")]
    public static async Task<string> ConfirmAction(
        McpServer server,
        [Description("Description of the action to confirm")] string action,
        [Description("Whether the action is destructive")] bool destructive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if client supports elicitation
            if (server.ClientCapabilities?.Elicitation == null)
            {
                return "Elicitation not supported by this client.";
            }

            // Build message with destructive warning if applicable
            var message = destructive
                ? $"⚠️ DESTRUCTIVE ACTION - Please confirm: {action}"
                : $"Please confirm: {action}";

            // Form elicitation: Display a structured form with typed fields
            // The client renders this as a dialog/form based on the JSON schema
            var result = await server.ElicitAsync(new ElicitRequestParams
            {
                Message = message,
                RequestedSchema = new ElicitRequestParams.RequestSchema
                {
                    Properties =
                    {
                        ["confirm"] = new ElicitRequestParams.BooleanSchema
                        {
                            Title = "Confirm",
                            Description = destructive
                                ? "Confirm this destructive action"
                                : "Confirm the action"
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
    /// Request feedback from the user
    /// </summary>
    [McpServerTool(
        Name = "get_feedback",
        Title = "Get Feedback",
        ReadOnly = true,
        Destructive = false,
        Idempotent = false,  // User response varies
        OpenWorld = true,    // Opens external URL
        IconSource = Icons.Speech)]
    [Description("Request feedback from the user")]
    public static async Task<string> GetFeedback(
        McpServer server,
        [Description("The question to ask the user")] string question,
        CancellationToken cancellationToken = default)
    {
        var feedbackUrl = "https://github.com/SamMorrowDrums/mcp-starters/issues/new?template=workshop-feedback.yml";
        if (!string.IsNullOrEmpty(question))
        {
            feedbackUrl += $"&title={Uri.EscapeDataString(question)}";
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
