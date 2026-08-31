namespace SchoolOperations.AI.Tools;

public class ToolResult
{
    // Indicates whether the tool executed successfully.
    public bool Success { get; set; }

    // Contains the tool's actual result data.
    public object? Data { get; set; }

    // Contains error information when the tool fails.
    public ToolError? Error { get; set; }


    // ---------------------------------------------------------
    // Creates a successful result
    // ---------------------------------------------------------

    public static ToolResult Ok(object data)
    {
        return new ToolResult
        {
            Success = true,
            Data = data,
            Error = null
        };
    }


    // ---------------------------------------------------------
    // Creates a failed result
    // ---------------------------------------------------------

    public static ToolResult Fail(
        string code,
        string message)
    {
        return new ToolResult
        {
            Success = false,
            Data = null,
            Error = new ToolError
            {
                Code = code,
                Message = message
            }
        };
    }
}


// =============================================================
// Common AI tool error
// =============================================================

public class ToolError
{
    // Machine-readable error code.
    public string Code { get; set; } = string.Empty;

    // Human-readable error message.
    public string Message { get; set; } = string.Empty;
}