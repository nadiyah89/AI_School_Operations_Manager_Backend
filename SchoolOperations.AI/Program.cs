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
// 6. Create HttpClient for AttendanceTool
// ---------------------------------------------------------

var httpClientFactory =
    serviceProvider
        .GetRequiredService<IHttpClientFactory>();

var attendanceHttpClient =
    httpClientFactory.CreateClient();


// ---------------------------------------------------------
// 7. Create AttendanceTool
// ---------------------------------------------------------

var attendanceTool =
    new AttendanceTool(
        attendanceHttpClient);


// ---------------------------------------------------------
// 8. Register AttendanceTool
// ---------------------------------------------------------

registry.Register(
    attendanceTool);


// ---------------------------------------------------------
// 9. Create AI Orchestrator
// ---------------------------------------------------------

var orchestrator =
    new AIOrchestrator(
        client,
        registry);


// ---------------------------------------------------------
// 10. Admin JWT
// ---------------------------------------------------------
//
// Get this from:
//
// POST /api/Auth/login
//
// Paste your Admin JWT below.
//
// DO NOT include "Bearer " here.
// ---------------------------------------------------------

Console.Write(
    "Enter JWT access token: ");

var accessToken =
    Console.ReadLine() ?? "";


// ---------------------------------------------------------
// 11. Send request to AI Agent
// ---------------------------------------------------------
//
// Gemini should understand that this request requires
// the AttendanceTool.
//
// Gemini should generate approximately:
//
// threshold = 75
//
// The AttendanceTool will then call the real backend.
// ---------------------------------------------------------

var result =
    await orchestrator.ProcessAsync(
          "Which students have attendance below 75%?",
        accessToken);


// ---------------------------------------------------------
// 12. Display final AI response
// ---------------------------------------------------------

Console.WriteLine();
Console.WriteLine("==========================================");
Console.WriteLine("AI Response");
Console.WriteLine("==========================================");

Console.WriteLine(result);

Console.WriteLine();