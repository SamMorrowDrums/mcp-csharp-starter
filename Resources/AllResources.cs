/*
 * MCP C# Starter - Resources
 *
 * Resources are read-only data that MCP servers expose to clients. Unlike tools
 * (which perform actions), resources provide context — files, documents, config,
 * or any data an AI assistant might need to answer questions.
 *
 * TWO KINDS OF RESOURCES:
 * - Static resources: Fixed URI, always available (e.g., about://server)
 * - Resource templates: URI with parameters, generates content on demand
 *   (e.g., greeting://{name} or item://{id})
 *
 * URI SCHEMES:
 * Resources use custom URI schemes (not http://). The scheme is arbitrary —
 * about://, doc://, greeting:// are conventions, not standards. Clients use
 * these URIs to request resource content from the server.
 *
 * HOW IT WORKS IN C#:
 * - [McpServerResourceType] marks a class as containing MCP resources
 * - [McpServerResource] marks a method and defines its URI, name, and MIME type
 * - Method parameters map to {placeholders} in UriTemplate for resource templates
 */

using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpCSharpStarter.Resources;

/// <summary>
/// All resources for the MCP server.
/// </summary>
[McpServerResourceType]
public class AllResources
{
    // =========================================================================
    // STATIC RESOURCES — Fixed content at a known URI
    // These appear in the resources/list response so clients know about them.
    // =========================================================================

    /// <summary>
    /// About resource — a simple static text resource at a fixed URI.
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
        SDK: ModelContextProtocol 1.0.0
        """;

    /// <summary>
    /// Example document — demonstrates serving document content as a resource.
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

    // =========================================================================
    // RESOURCE TEMPLATES — Parameterized URIs that generate content on demand
    // Templates use {placeholders} in the URI. The client fills in the values
    // and the server generates content dynamically. These appear in the
    // resources/templates/list response.
    // =========================================================================

    /// <summary>
    /// Personalized greeting — a resource template with a {name} parameter.
    /// Requesting greeting://Alice returns a greeting for Alice.
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
    /// Item data — a resource template returning JSON content.
    /// Demonstrates using MimeType = "application/json" for structured data.
    /// </summary>
    [McpServerResource(
        UriTemplate = "item://{id}",
        Name = "Item Data",
        Title = "Item Data",
        MimeType = "application/json")]
    [Description("Data for a specific item by ID")]
    public static string ItemData(string id)
    {
        var items = new Dictionary<string, object>
        {
            ["1"] = new { id = "1", name = "Widget", description = "A standard widget", category = "tools" },
            ["2"] = new { id = "2", name = "Gadget", description = "A fancy gadget", category = "electronics" },
            ["3"] = new { id = "3", name = "Doohickey", description = "A mysterious doohickey", category = "misc" }
        };

        var itemData = items.TryGetValue(id, out var item)
            ? item
            : new { id = id, name = $"Item {id}", description = $"Unknown item with ID {id}", category = "unknown" };

        return JsonSerializer.Serialize(itemData, new JsonSerializerOptions { WriteIndented = true });
    }
}
