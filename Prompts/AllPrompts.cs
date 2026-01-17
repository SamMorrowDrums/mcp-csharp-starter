/*
 * MCP C# Starter - Prompts
 *
 * Demonstrates various prompt patterns in MCP:
 * - Basic prompts with arguments
 * - Prompts with argument completions
 */

using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace McpCSharpStarter.Prompts;

/// <summary>
/// All prompts for the MCP server
/// </summary>
[McpServerPromptType]
public class AllPrompts
{
    /// <summary>
    /// Generate a greeting in a specific style
    /// </summary>
    [McpServerPrompt(Name = "greet", Title = "Greeting Prompt")]
    [Description("Generate a personalized greeting message with customizable style")]
    public static IEnumerable<PromptMessage> Greet(
        [Description("Name of the person to greet")] string name,
        [Description("The greeting style (formal, casual, enthusiastic)")] string style = "casual")
    {
        var styles = new Dictionary<string, string>
        {
            ["formal"] = $"Please compose a formal, professional greeting for {name}.",
            ["casual"] = $"Write a casual, friendly hello to {name}.",
            ["enthusiastic"] = $"Create an excited, enthusiastic greeting for {name}!"
        };

        var text = styles.GetValueOrDefault(style, styles["casual"]);

        return [
            new PromptMessage
            {
                Role = Role.User,
                Content = new TextContentBlock { Text = text }
            }
        ];
    }

    /// <summary>
    /// Request a code review with specific focus areas
    /// </summary>
    [McpServerPrompt(Name = "code_review", Title = "Code Review")]
    [Description("Request a code review with specific focus areas")]
    public static IEnumerable<PromptMessage> CodeReview(
        [Description("The code to review")] string code,
        [Description("Programming language")] string language,
        [Description("What to focus on (security, performance, readability, all)")] string focus = "all")
    {
        var focusInstructions = new Dictionary<string, string>
        {
            ["security"] = "Focus on security vulnerabilities and potential exploits.",
            ["performance"] = "Focus on performance optimizations and efficiency issues.",
            ["readability"] = "Focus on code clarity, naming, and maintainability.",
            ["all"] = "Provide a comprehensive review covering security, performance, and readability."
        };

        var instruction = focusInstructions.GetValueOrDefault(focus, focusInstructions["all"]);

        var text = $"""
            Please review the following {language} code. {instruction}

            ```{language}
            {code}
            ```
            """;

        return [
            new PromptMessage
            {
                Role = Role.User,
                Content = new TextContentBlock { Text = text }
            }
        ];
    }
}
