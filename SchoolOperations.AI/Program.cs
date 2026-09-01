using Google.GenAI;
using Microsoft.Extensions.DependencyInjection;
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

var client =
    new Client(apiKey: apiKey);


// ---------------------------------------------------------
// 3. Create dependency injection services
// ---------------------------------------------------------

var services =
    new ServiceCollection();

services.AddHttpClient();

var serviceProvider =
    services.BuildServiceProvider();


// ---------------------------------------------------------
// 4. Create Tool Registry
// ---------------------------------------------------------

var registry =
    new ToolRegistry();


// ---------------------------------------------------------
// 5. Register School Name Tool
// ---------------------------------------------------------

registry.Register(
    new SchoolNameTool());


// ---------------------------------------------------------
// 6. Create HttpClientFactory
// ---------------------------------------------------------

var httpClientFactory =
    serviceProvider
        .GetRequiredService<IHttpClientFactory>();


// ---------------------------------------------------------
// 7. Create and register AttendanceTool
// ---------------------------------------------------------

var attendanceTool =
    new AttendanceTool(
        httpClientFactory.CreateClient());

registry.Register(
    attendanceTool);


// ---------------------------------------------------------
// 8. Create and register PoorPerformanceTool
// ---------------------------------------------------------

var poorPerformanceTool =
    new PoorPerformanceTool(
        httpClientFactory.CreateClient());

registry.Register(
    poorPerformanceTool);


// ---------------------------------------------------------
// 9. Create and register DecliningPerformanceTool
// ---------------------------------------------------------

var decliningPerformanceTool =
    new DecliningPerformanceTool(
        httpClientFactory.CreateClient());

registry.Register(
    decliningPerformanceTool);


// ---------------------------------------------------------
// 10. Create AI Orchestrator
// ---------------------------------------------------------

var orchestrator =
    new AIOrchestrator(
        client,
        registry);


// ---------------------------------------------------------
// 11. Read user JWT
// ---------------------------------------------------------
//
// Get this from:
//
// POST /api/Auth/login
//
// Do NOT include "Bearer ".
// ---------------------------------------------------------

Console.Write(
    "Enter JWT access token: ");

var accessToken =
    Console.ReadLine() ?? "";


// ---------------------------------------------------------
// 12. Send request to AI Agent
// ---------------------------------------------------------

var result =
    await orchestrator.ProcessAsync(
        "Show me students who scored below 60% academically " +
        "and also students with declining academic performance.",
        accessToken);


// ---------------------------------------------------------
// 13. Display final AI response
// ---------------------------------------------------------

Console.WriteLine();

Console.WriteLine(
    "==========================================");

Console.WriteLine(
    "AI Response");

Console.WriteLine(
    "==========================================");

Console.WriteLine(result);

Console.WriteLine();

