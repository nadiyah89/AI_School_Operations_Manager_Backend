using Google.GenAI;
using SchoolOperations.AI.Orchestration;
using SchoolOperations.AI.Tools;

// ---------------------------------------------------------
// 1. Read Gemini API key
// ---------------------------------------------------------

var apiKey =
    Environment.GetEnvironmentVariable("GEMINI_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("GEMINI_API_KEY is not found.");
    return;
}

// ---------------------------------------------------------
// 2. Create Gemini client
// ---------------------------------------------------------

var client = new Client(apiKey: apiKey);

// ---------------------------------------------------------
// 3. Create and configure tool registry
// ---------------------------------------------------------

var registry = new ToolRegistry();

registry.Register(new SchoolNameTool());

// ---------------------------------------------------------
// 4. Create orchestrator
// ---------------------------------------------------------

var orchestrator =
    new AIOrchestrator(client, registry);

// ---------------------------------------------------------
// 5. Send request to orchestrator
// ---------------------------------------------------------

var result = await orchestrator.ProcessAsync(
    "What is the name of our school?");

// ---------------------------------------------------------
// 6. Display response
// ---------------------------------------------------------

Console.WriteLine(result);