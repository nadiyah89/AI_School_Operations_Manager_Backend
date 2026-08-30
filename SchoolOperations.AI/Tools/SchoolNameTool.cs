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

    public Task<object> ExecuteAsync(
        Dictionary<string, object> arguments)
    {
        return Task.FromResult<object>(
            "ABC International School");
    }
}