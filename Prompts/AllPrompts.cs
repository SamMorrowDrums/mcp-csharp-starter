/*
 * MCP C# Starter - Prompts
 *
 * Prompts are reusable message templates that clients can retrieve and send to
 * an LLM. Unlike tools (server executes code) or resources (server returns data),
 * prompts provide pre-written instructions the *client* sends to its LLM.
 *
 * USE CASES:
 * - Standardized workflows (code review, greeting generation)
 * - Complex multi-step instructions the user doesn't want to type every time
 * - Parameterized templates with arguments filled in at request time
 *
 * HOW IT WORKS IN C#:
 * - [McpServerPromptType] marks a class as containing MCP prompts
 * - [McpServerPrompt] marks a method as an MCP prompt
 * - Methods return IEnumerable<PromptMessage> — the messages to send to the LLM
 * - Parameters become prompt arguments the client fills in
 */

using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace McpCSharpStarter.Prompts;

/// <summary>
/// All prompts for the MCP server.
/// Each method returns messages that the client sends to its LLM.
/// </summary>
[McpServerPromptType]
public class AllPrompts
{
    /// <summary>
    /// Greeting prompt — demonstrates a prompt with required and optional arguments.
    /// The "style" parameter is optional, showing how prompts can have defaults.
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
    /// Code review prompt — demonstrates a structured multi-step instruction.
    /// The LLM receives a detailed review checklist along with the user's code.
    /// </summary>
    [McpServerPrompt(Name = "code_review", Title = "Code Review")]
    [Description("Review code for potential improvements")]
    public static IEnumerable<PromptMessage> CodeReview(
        [Description("Code to review")] string code)
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
