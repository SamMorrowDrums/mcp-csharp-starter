using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

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
        UriTemplate = "about://server",
        Name = "About",
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
        UriTemplate = "doc://example",
        Name = "Example Document",
        Title = "Example Document",
        MimeType = "text/plain")]
    [Description("An example document resource")]
    public static string ExampleDocument() => """
        # Sample Document
        
        This is a sample document served as an MCP resource.
        
        ## Features
        
        - **Resources** can serve any content type
        - **MIME types** help clients understand content
        - **URIs** provide unique identifiers
        
        ## Usage
        
        Clients can read this resource to get sample content for testing
        or demonstration purposes.
        
        ## Conclusion
        
        Resources are a powerful way to expose data to MCP clients!
        """;

    /// <summary>
    /// Personalized greeting resource template.
    /// Demonstrates parameterized resources.
    /// </summary>
    [McpServerResource(
        UriTemplate = "greeting://{name}",
        Name = "Personalized Greeting",
        Title = "Personalized Greeting",
        MimeType = "text/plain")]
    [Description("A personalized greeting for a specific person")]
    public static string PersonalizedGreeting(string name) => 
        $"Hello, {name}! This is a personalized greeting generated just for you.";

    /// <summary>
    /// Item data resource template.
    /// Demonstrates JSON resource with parameters.
    /// </summary>
    [McpServerResource(
        UriTemplate = "item://{id}",
        Name = "Item Data",
        Title = "Item Data",
        MimeType = "application/json")]
    [Description("Data for a specific item by ID")]
    public static string ItemData(string id)
    {
        var itemData = new
        {
            id = id,
            name = $"Item {id}",
            description = $"This is a sample item with ID {id}",
            timestamp = DateTime.UtcNow.ToString("o")
        };
        return JsonSerializer.Serialize(itemData, new JsonSerializerOptions { WriteIndented = true });
    }
}
