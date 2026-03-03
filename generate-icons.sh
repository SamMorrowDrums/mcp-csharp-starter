#!/bin/bash
# Script to regenerate Icons.cs from PNG files in Icons/ directory
# Run with: ./generate-icons.sh

set -e

OUTPUT_FILE="Icons.cs"
ICONS_DIR="Icons"

cat > "$OUTPUT_FILE" << 'HEADER'
namespace McpCSharpStarter;

/// <summary>
/// Icon constants for MCP tools
/// From Microsoft Fluent UI Emoji (MIT License)
/// 
/// NOTE: This file is auto-generated from PNG files in the Icons/ directory.
/// To regenerate, run: ./generate-icons.sh
/// </summary>
public static class Icons
{
HEADER

# Generate each icon constant
for icon_file in "$ICONS_DIR"/*.png; do
    filename=$(basename "$icon_file" .png)
    # Convert snake_case to PascalCase
    const_name=$(echo "$filename" | sed -r 's/(^|_)([a-z])/\U\2/g')
    base64_data=$(base64 -w 0 "$icon_file")
    echo "    public const string $const_name = \"data:image/png;base64,$base64_data\";" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
done

cat >> "$OUTPUT_FILE" << 'FOOTER'
    // Emoji fallbacks for icons without PNG files
    public const string Question = "❓";
    public const string Speech = "💬";
}
FOOTER

echo "Generated $OUTPUT_FILE from PNG files in $ICONS_DIR/"
