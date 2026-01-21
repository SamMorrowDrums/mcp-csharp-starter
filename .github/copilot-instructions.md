# MCP C# Starter - Copilot Coding Agent Instructions

## Building and Testing

- **Restore dependencies:**
  ```bash
  dotnet restore
  ```

- **Build the project:**
  ```bash
  dotnet build
  ```

- **Run the server:**
  ```bash
  dotnet run
  ```

- **Run tests:**
  ```bash
  dotnet test
  ```

- **Format code:**
  ```bash
  dotnet format
  ```

- **Publish release:**
  ```bash
  dotnet publish -c Release
  ```

## Code Conventions

- Follow C# naming conventions (PascalCase for public members)
- Use `async/await` for asynchronous operations
- Use nullable reference types (`?` suffix)
- Prefer expression-bodied members for simple methods

## Before Committing Checklist

1. ✅ Run `dotnet format` to format code
2. ✅ Run `dotnet build` to verify compilation
3. ✅ Run `dotnet test` to verify tests pass
4. ✅ Test the server with `dotnet run`

