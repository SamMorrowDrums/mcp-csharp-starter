using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ModelContextProtocol.Client;

const string LegacyProtocolVersion = "2025-11-25";

var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
var port = GetAvailablePort();
var server = StartServer(repositoryRoot, port);

try
{
    await WaitForServerAsync(port, server);
    var endpoint = new Uri($"http://localhost:{port}/");

    await VerifyAutomaticDowngradeAsync(endpoint);
    await VerifyLegacySessionAsync(endpoint);
    Console.WriteLine("HTTP protocol downgrade and legacy session checks passed.");
}
finally
{
    if (!server.HasExited)
    {
        server.Kill(entireProcessTree: true);
        await server.WaitForExitAsync();
    }
}

static int GetAvailablePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static Process StartServer(string repositoryRoot, int port)
{
    var projectPath = Path.Combine(repositoryRoot, "McpCSharpStarter.csproj");
    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = repositoryRoot,
        UseShellExecute = false
    };
    startInfo.ArgumentList.Add("run");
    startInfo.ArgumentList.Add("--no-build");
    startInfo.ArgumentList.Add("--project");
    startInfo.ArgumentList.Add(projectPath);
    startInfo.ArgumentList.Add("--");
    startInfo.ArgumentList.Add("--http");
    startInfo.ArgumentList.Add("--port");
    startInfo.ArgumentList.Add(port.ToString());

    return Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start the MCP server.");
}

static async Task WaitForServerAsync(int port, Process server)
{
    using var httpClient = new HttpClient();
    var healthUrl = $"http://localhost:{port}/health";
    var deadline = DateTime.UtcNow.AddSeconds(30);

    while (DateTime.UtcNow < deadline)
    {
        if (server.HasExited)
        {
            throw new InvalidOperationException($"MCP server exited with code {server.ExitCode}.");
        }

        try
        {
            using var response = await httpClient.GetAsync(healthUrl);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
        }
        catch (HttpRequestException)
        {
        }

        await Task.Delay(200);
    }

    throw new TimeoutException("MCP server did not become healthy within 30 seconds.");
}

static async Task VerifyAutomaticDowngradeAsync(Uri endpoint)
{
    await using var client = await McpClient.CreateAsync(
        new HttpClientTransport(new HttpClientTransportOptions { Endpoint = endpoint }));

    if (client.NegotiatedProtocolVersion != LegacyProtocolVersion || client.SessionId is null)
    {
        throw new InvalidOperationException(
            $"A default modern client should downgrade to a stateful {LegacyProtocolVersion} session; " +
            $"received version {client.NegotiatedProtocolVersion} with session ID " +
            $"{(client.SessionId is null ? "missing" : "present")}.");
    }

    await client.CallToolAsync("load_bonus_tool", new Dictionary<string, object?>());
    var tools = await client.ListToolsAsync();
    if (!tools.Any(tool => tool.Name == "bonus_calculator"))
    {
        throw new InvalidOperationException("Dynamically loaded tools did not persist across HTTP requests.");
    }
}

static async Task VerifyLegacySessionAsync(Uri endpoint)
{
    await using var client = await McpClient.CreateAsync(
        new HttpClientTransport(new HttpClientTransportOptions { Endpoint = endpoint }),
        new McpClientOptions { ProtocolVersion = LegacyProtocolVersion });

    if (client.NegotiatedProtocolVersion != LegacyProtocolVersion || client.SessionId is null)
    {
        throw new InvalidOperationException("An explicit legacy client did not receive a stateful HTTP session.");
    }

    var tools = await client.ListToolsAsync();
    if (!tools.Any(tool => tool.Name == "confirm_action") || !tools.Any(tool => tool.Name == "get_feedback"))
    {
        throw new InvalidOperationException("Legacy session-dependent elicitation tools are not advertised.");
    }
}
