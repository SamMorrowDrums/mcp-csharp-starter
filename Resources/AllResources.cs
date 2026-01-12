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

    /// <summary>
    /// Sample document resource - demonstrates file-like resource.
    /// Shows how to serve document content.
    /// </summary>
    [McpServerResource(
        UriTemplate = "file://example.md",
        Name = "sample_document",
        Title = "Sample Document",
        MimeType = "text/markdown")]
    [Description("A sample markdown document demonstrating resource capabilities")]
    public static string SampleDocument() => """
        # Sample Markdown Document
        
        This is a sample document served as an MCP resource.
        
        ## Features
        
        - **Resources** can serve any content type
        - **MIME types** help clients understand content
        - **URIs** provide unique identifiers
        
        ## Usage
        
        Clients can read this resource to get sample content for testing
        or demonstration purposes.
        
        ```csharp
        // Example code block
        var client = new McpClient();
        var content = await client.ReadResourceAsync("file://example.md");
        ```
        
        ## Conclusion
        
        Resources are a powerful way to expose data to MCP clients!
        """;
}
