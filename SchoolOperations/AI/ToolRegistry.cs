using Google.GenAI.Types;

namespace SchoolOperations.AI;

public class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools;


    // ---------------------------------------------------------
    // Constructor
    // Automatically receives all registered ITool implementations
    // ---------------------------------------------------------

    public ToolRegistry(IEnumerable<ITool> tools)
    {
        _tools = new Dictionary<string, ITool>();

        foreach (var tool in tools)
        {
            _tools[tool.Name] = tool;
        }
    }


    // ---------------------------------------------------------
    // Register a tool
    // Retained for compatibility
    // ---------------------------------------------------------

    public void Register(ITool tool)
    {
        _tools[tool.Name] = tool;
    }


    // ---------------------------------------------------------
    // Find a tool by name
    // ---------------------------------------------------------

    public ITool? GetTool(string name)
    {
        _tools.TryGetValue(
            name,
            out var tool);

        return tool;
    }


    // ---------------------------------------------------------
    // Create Gemini tool declarations
    // ---------------------------------------------------------

    public Tool CreateGeminiTool()
    {
        var declarations =
            new List<FunctionDeclaration>();

        foreach (var tool in _tools.Values)
        {
            declarations.Add(
                new FunctionDeclaration
                {
                    Name = tool.Name,

                    Description = tool.Description,

                    Parameters = new Schema
                    {
                        Type = "object",

                        Properties = tool.Parameters
                    }
                });
        }

        return new Tool
        {
            FunctionDeclarations = declarations
        };
    }
}