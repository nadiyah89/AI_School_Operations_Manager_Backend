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

    public async Task<string> ProcessAsync(string userMessage)
    {
        // ---------------------------------------------------------
        // 1. System instruction
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

                When you need the official school name,
                use the GetSchoolName tool.
                """
            }
            ]
        };

        // ---------------------------------------------------------
        // 2. Create conversation history
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
        // 3. Maximum number of agent iterations
        // ---------------------------------------------------------

        const int maxIterations = 5;

        // ---------------------------------------------------------
        // 4. Agent loop
        // ---------------------------------------------------------

        for (int iteration = 1;
             iteration <= maxIterations;
             iteration++)
        {
            Console.WriteLine(
                $"\nAgent iteration: {iteration}");

            // -----------------------------------------------------
            // 5. Ask Gemini what to do next
            // -----------------------------------------------------

            var response =
                await _client.Models.GenerateContentAsync(
                    model: "gemini-3.6-flash",
                    contents: contents,
                    config: new GenerateContentConfig
                    {
                        SystemInstruction = systemInstruction,

                        Tools =
                        [
                            _toolRegistry.CreateGeminiTool()
                        ]
                    });

            // -----------------------------------------------------
            // 6. Preserve Gemini's response
            // -----------------------------------------------------

            var modelContent =
                response.Candidates?[0].Content;

            if (modelContent != null)
            {
                contents.Add(modelContent);
            }

            // -----------------------------------------------------
            // 7. Check whether Gemini requested a tool
            // -----------------------------------------------------

            var functionCalls =
                response.FunctionCalls ?? [];

            // -----------------------------------------------------
            // 8. No tool call means the agent is finished
            // -----------------------------------------------------

            if (functionCalls.Count == 0)
            {
                return response.Text ??
                       "No response received.";
            }

            // -----------------------------------------------------
            // 9. Execute requested tools
            // -----------------------------------------------------

            foreach (var functionCall in functionCalls)
            {
                if (string.IsNullOrWhiteSpace(functionCall.Name))
                {
                    continue;
                }

                Console.WriteLine(
                    $"Gemini requested tool: {functionCall.Name}");

                // -------------------------------------------------
                // 10. Find the tool through our registry
                // -------------------------------------------------

                var tool =
                    _toolRegistry.GetTool(functionCall.Name);

                if (tool == null)
                {
                    Console.WriteLine(
                        $"Tool '{functionCall.Name}' " +
                        "is not registered.");

                    continue;
                }

                // -------------------------------------------------
                // 11. Read tool arguments
                // -------------------------------------------------

                var arguments =
                    functionCall.Args ??
                    new Dictionary<string, object>();

                // -------------------------------------------------
                // 12. Execute the actual application tool
                // -------------------------------------------------

                var result =
                    await tool.ExecuteAsync(arguments);

                Console.WriteLine(
                    $"Tool result: {result}");

                // -------------------------------------------------
                // 13. Send tool result back to Gemini
                // -------------------------------------------------

                var toolResponse = new Content
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
                                        ["result"] = result
                                    }
                            }
                    }
                    ]
                };

                contents.Add(toolResponse);
            }
        }

        // ---------------------------------------------------------
        // 14. Agent reached the safety limit
        // ---------------------------------------------------------

        return "The AI agent reached its maximum number of steps.";
    }

   
}