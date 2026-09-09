using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using SchoolOperations.AI.Tools;

namespace SchoolOperations.AI.Orchestration;

public class AIOrchestrator
{
    private readonly Client _client;
    private readonly ToolRegistry _toolRegistry;

    private readonly ILogger<AIOrchestrator> _logger;

    public AIOrchestrator(
    Client client,
    ToolRegistry toolRegistry,
    ILogger<AIOrchestrator> logger)
    {
        _client = client;
        _toolRegistry = toolRegistry;
        _logger = logger;
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

                    Do not assume or invent a currency symbol or currency
                    unless the tool data explicitly provides one.
                    
                    When displaying monetary values without currency information,
                    show the numeric amount without adding symbols such as $, ₹, or €.

                    When a tool returns a successful result,
                    use the returned data to answer the user.

                    When a tool returns an error,
                    explain the error clearly to the user.

                    Do not pretend that a tool succeeded
                    when it returned an error.

                    When a user's request is ambiguous between multiple
                    available tools or business meanings, ask a clarifying
                    question instead of guessing.

                    Do not invent missing thresholds, filters, or business rules.

                    Only call a tool when the user's request clearly matches
                    that tool's capability and the required information is available.

                    When you use a tool to create a notification,
                    you must inform the user that the notification has been
                    created and recorded successfully.

                    You must never claim that an SMS or Email was physically sent,
                    because the system only records notifications in the database.

                    
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
            _logger.LogInformation(
    "            AI agent iteration: {Iteration}",
                 iteration);


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

                _logger.LogError(
                       ex,
                      "Gemini API error occurred.");

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

            // -----------------------------------------------------
            // 10. Collect all tool responses for this Gemini turn
            // -----------------------------------------------------

            var responseParts = new List<Part>();

            foreach (var functionCall in functionCalls)
            {
                if (string.IsNullOrWhiteSpace(
                    functionCall.Name))
                {
                    continue;
                }


                _logger.LogInformation(
                  "Gemini requested tool: {ToolName}",
                   functionCall.Name);


                // -------------------------------------------------
                // 11. Find tool in registry
                // -------------------------------------------------

                var tool =
                    _toolRegistry.GetTool(
                        functionCall.Name);

                if (tool == null)
                {
                    _logger.LogWarning(
                      "Requested tool {ToolName} is not registered.",
                       functionCall.Name);

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

                    _logger.LogError(
                                ex,
                         "Tool execution failed for {ToolName}.",
                         functionCall.Name);

                    result =
                        ToolResult.Fail(
                            "ToolExecutionError",
                            "The tool could not complete the requested operation.");
                }


                // ---------------------------------------------------------
                // 14. Log result status
                // ---------------------------------------------------------

                _logger.LogInformation(
                 "Tool {ToolName} completed. Success: {Success}",
                   functionCall.Name,
                   result.Success);

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

                // -------------------------------------------------
                // 17. Add this tool response to the current turn
                // -------------------------------------------------

                responseParts.Add(
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
                    });
            }

            // -----------------------------------------------------
            // 18. Add all tool responses as one Gemini turn
            // -----------------------------------------------------

            if (responseParts.Count > 0)
            {
                contents.Add(
                    new Content
                    {
                        Parts =
                            responseParts
                    });
            }
        }

        
        // ---------------------------------------------------------
        // 19. Safety limit
        // ---------------------------------------------------------

        return
            "The AI agent reached its maximum number of steps.";
    }
}