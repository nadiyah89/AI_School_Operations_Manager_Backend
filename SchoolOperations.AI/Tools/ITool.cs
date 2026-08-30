using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public interface ITool
{
    string Name { get; }

    string Description { get; }

    Dictionary<string, Schema> Parameters { get; }

    Task<object> ExecuteAsync(
        Dictionary<string, object> arguments);
}