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
    /// Generate a greeting message
    /// </summary>
    [McpServerPrompt(Name = "greet", Title = "Greeting Prompt")]
    [Description("Generate a greeting message")]
    public static IEnumerable<PromptMessage> Greet(
        [Description("Name of the person to greet")] string name,
        [Description("Greeting style (formal/casual)")] string? style = null)
    {
        var styleText = style switch
        {
            "formal" => "formal, professional",
            "casual" => "casual, friendly",
            _ => "warm and friendly"
        };

        var text = $"Please compose a {styleText} greeting for {name}.";

        return [
            new PromptMessage
            {
                Role = Role.User,
                Content = new TextContentBlock { Text = text }
            }
        ];
    }

    /// <summary>
    /// Review code for potential improvements
    /// </summary>
    [McpServerPrompt(Name = "code_review", Title = "Code Review")]
    [Description("Review code for potential improvements")]
    public static IEnumerable<PromptMessage> CodeReview(
        [Description("The code to review")] string code)
    {
        var text = $"""
            Please review the following code for potential improvements, focusing on:
            - Security vulnerabilities
            - Performance issues
            - Code quality and maintainability
            - Best practices

            ```
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
