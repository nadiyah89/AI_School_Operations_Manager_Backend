using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class SchoolNameTool : ITool
{
    public string Name =>
        "GetSchoolName";

    public string Description =>
        "Gets the official name of the school.";

    public Dictionary<string, Schema> Parameters =>
        new();


    // =========================================================
    // Execute School Name Tool
    // =========================================================

    public Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // The tool successfully retrieved the school name.
        return Task.FromResult(
            ToolResult.Ok(
                "ABC International School"));
    }
}