using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using SchoolOperations.AI.Tools;

namespace SchoolOperations.AI.Orchestration;

public class AIOrchestrator
{
    private readonly Client _client;
    private readonly ToolRegistry _toolRegistry;

    public AIOrchestrator(
        Client client,
        ToolRegistry toolRegistry)
    {
        _client = client;
        _toolRegistry = toolRegistry;
    }

    public async Task<string> ProcessAsync(
        string userMessage,
        string accessToken)
    {
        // ---------------------------------------------------------
        // 1. Create context for this AI request
        // ---------------------------------------------------------

        var context =
            new AIToolContext(accessToken);


        // ---------------------------------------------------------
        // 2. System instruction
        // ---------------------------------------------------------

        var systemInstruction = new Content
        {
            Parts =
            [
                new Part
                {
                    Text = """
                    You are the AI assistant for a school operations system.

                    Your job is to help authorized school staff.

                    Be concise and professional.

                    Do not invent school data.

                    When a tool returns a successful result,
                    use the returned data to answer the user.

                    When a tool returns an error,
                    explain the error clearly to the user.

                    Do not pretend that a tool succeeded
                    when it returned an error.
                    """
                }
            ]
        };


        // ---------------------------------------------------------
        // 3. Create conversation history
        // ---------------------------------------------------------

        var contents = new List<Content>
        {
            new Content
            {
                Parts =
                [
                    new Part
                    {
                        Text = userMessage
                    }
                ]
            }
        };


        // ---------------------------------------------------------
        // 4. Maximum agent iterations
        // ---------------------------------------------------------

        const int maxIterations = 5;


        // ---------------------------------------------------------
        // 5. Agent loop
        // ---------------------------------------------------------

        for (
            int iteration = 1;
            iteration <= maxIterations;
            iteration++)
        {
            Console.WriteLine(
                $"\nAgent iteration: {iteration}");


            // -----------------------------------------------------
            // 6. Ask Gemini what to do next
            // -----------------------------------------------------

            GenerateContentResponse response;

            try
            {
                response =
                    await _client.Models.GenerateContentAsync(
                        model: "gemini-3.6-flash",
                        contents: contents,
                        config: new GenerateContentConfig
                        {
                            SystemInstruction =
                                systemInstruction,

                            Tools =
                            [
                                _toolRegistry.CreateGeminiTool()
                            ]
                        });
            }
            catch (Exception ex)
            {
                // ---------------------------------------------------------
                // Gemini API failure
                // ---------------------------------------------------------
                // This protects the application from quota,
                // network, timeout, and other unexpected Gemini errors.

                Console.WriteLine(
                    $"Gemini API error: {ex.Message}");

                return
                    "The AI service is currently unavailable. " +
                    "Please try again later.";
            }


            // -----------------------------------------------------
            // 7. Preserve Gemini response
            // -----------------------------------------------------

            var modelContent =
                response.Candidates?[0].Content;

            if (modelContent != null)
            {
                contents.Add(modelContent);
            }


            // -----------------------------------------------------
            // 8. Check for function calls
            // -----------------------------------------------------

            var functionCalls =
                response.FunctionCalls ?? [];


            // -----------------------------------------------------
            // 9. No tool call = final answer
            // -----------------------------------------------------

            if (functionCalls.Count == 0)
            {
                return response.Text ??
                       "No response received.";
            }


            // -----------------------------------------------------
            // 10. Execute requested tools
            // ---------------------------------------------------------

            foreach (var functionCall in functionCalls)
            {
                if (string.IsNullOrWhiteSpace(
                    functionCall.Name))
                {
                    continue;
                }


                Console.WriteLine(
                    $"Gemini requested tool: " +
                    $"{functionCall.Name}");


                // -------------------------------------------------
                // 11. Find tool in registry
                // -------------------------------------------------

                var tool =
                    _toolRegistry.GetTool(
                        functionCall.Name);

                if (tool == null)
                {
                    Console.WriteLine(
                        $"Tool '{functionCall.Name}' " +
                        "is not registered.");

                    continue;
                }


                // -------------------------------------------------
                // 12. Read Gemini arguments
                // -------------------------------------------------

                var arguments =
                    functionCall.Args ??
                    new Dictionary<string, object>();


                // -------------------------------------------------
                // 13. Execute tool
                // -------------------------------------------------

                ToolResult result;

                try
                {
                    // Execute the requested AI tool.
                    result =
                        await tool.ExecuteAsync(
                            arguments,
                            context);
                }
                catch (Exception ex)
                {
                    // ---------------------------------------------------------
                    // Unexpected tool failure
                    // ---------------------------------------------------------
                    // Expected application errors should already be returned
                    // through ToolResult.Fail().
                    //
                    // This catch protects the agent from unexpected exceptions.

                    Console.WriteLine(
                        $"Tool execution failed: {ex.Message}");

                    result =
                        ToolResult.Fail(
                            "ToolExecutionError",
                            "The tool could not complete the requested operation.");
                }


                // ---------------------------------------------------------
                // 14. Log result status
                // ---------------------------------------------------------

                Console.WriteLine(
                    $"Tool success: {result.Success}");

                // -------------------------------------------------
                // 15. Convert ToolResult to JSON
                // -------------------------------------------------

                var json =
                    JsonSerializer.Serialize(result);


                // -------------------------------------------------
                // 16. Convert JSON into a normal object
                // -------------------------------------------------
                // Gemini needs structured JSON data rather than
                // the C# object's ToString() representation.

                var toolResultForGemini =
                    JsonSerializer.Deserialize<object>(
                        json);


                // -------------------------------------------------
                // 17. Send ToolResult back to Gemini
                // -------------------------------------------------

                var toolResponse =
                    new Content
                    {
                        Parts =
                        [
                            new Part
                            {
                                FunctionResponse =
                                    new FunctionResponse
                                    {
                                        Name =
                                            functionCall.Name,

                                        Response =
                                            new Dictionary<string, object>
                                            {
                                                ["result"] =
                                                    toolResultForGemini!
                                            }
                                    }
                            }
                        ]
                    };


                contents.Add(toolResponse);
            }
        }


        // ---------------------------------------------------------
        // 18. Safety limit
        // ---------------------------------------------------------

        return
            "The AI agent reached its maximum number of steps.";
    }
}