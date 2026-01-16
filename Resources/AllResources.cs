using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpCSharpStarter.Resources;

/// <summary>
/// Resource handlers for the MCP server.
/// Resources provide data that can be read by clients and referenced by tools.
/// </summary>
[McpServerResourceType]
public class AllResources
{
    /// <summary>
    /// About resource - provides server information.
    /// Demonstrates a simple text resource.
    /// </summary>
    [McpServerResource(
        UriTemplate = "info://about",
        Name = "about",
        Title = "About this Server",
        MimeType = "text/plain")]
    [Description("Information about this MCP server")]
    public static string About() => """
        MCP C# Starter Server
        ======================
        
        This is a sample Model Context Protocol server implemented in C#.
        It demonstrates:
        - Tool registration and execution
        - Resource handling
        - Prompt templates
        - Server configuration
        
        Version: 1.0.0
        Framework: .NET 8.0
        SDK: ModelContextProtocol 0.5.0-preview.1
        """;


}
