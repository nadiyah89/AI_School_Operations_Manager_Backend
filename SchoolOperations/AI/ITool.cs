using Google.GenAI.Types;
using SchoolOperations.AI.Tools;

namespace SchoolOperations.AI;

public interface ITool
{
    // Unique name Gemini uses to identify the tool.
    string Name { get; }

    // Description Gemini uses to understand the tool.
    string Description { get; }

    // Parameters expected by the tool.
    Dictionary<string, Schema> Parameters { get; }

    // Every AI tool now follows the common ToolResult contract.
    Task<ToolResult> ExecuteAsync(
    Dictionary<string, object> arguments,
    AIToolContext context);
}