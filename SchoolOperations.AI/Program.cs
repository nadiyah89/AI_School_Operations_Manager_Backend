using System.Text.Json;
using Google.GenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SchoolOperations.AI.Orchestration;
using SchoolOperations.AI.Tools;

//// ---------------------------------------------------------
//// 1. Read Gemini API key
//// ---------------------------------------------------------

var apiKey =
    Environment.GetEnvironmentVariable("GEMINI_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("GEMINI_API_KEY is not found.");
    return;
}


//// ---------------------------------------------------------
//// 2. Create Gemini client
//// ---------------------------------------------------------

var client =
    new Client(apiKey: apiKey);


//// ---------------------------------------------------------
//// 3. Create dependency injection services
//// ---------------------------------------------------------

var services =
    new ServiceCollection();

services.AddHttpClient();

var serviceProvider =
    services.BuildServiceProvider();


//// ---------------------------------------------------------
//// 4. Create Tool Registry
//// ---------------------------------------------------------

var registry =
    new ToolRegistry();


//// ---------------------------------------------------------
//// 5. Register School Name Tool
//// ---------------------------------------------------------

registry.Register(
    new SchoolNameTool());


//// ---------------------------------------------------------
//// 6. Create HttpClientFactory
//// ---------------------------------------------------------

var httpClientFactory =
    serviceProvider
        .GetRequiredService<IHttpClientFactory>();


//// ---------------------------------------------------------
//// 7. Create and register AttendanceTool
//// ---------------------------------------------------------

var attendanceTool =
    new AttendanceTool(
        httpClientFactory.CreateClient());

registry.Register(
    attendanceTool);


//// ---------------------------------------------------------
//// 8. Create and register PoorPerformanceTool
//// ---------------------------------------------------------

var poorPerformanceTool =
    new PoorPerformanceTool(
        httpClientFactory.CreateClient());

registry.Register(
    poorPerformanceTool);


//// ---------------------------------------------------------
//// 9. Create and register DecliningPerformanceTool
//// ---------------------------------------------------------

var decliningPerformanceTool =
    new DecliningPerformanceTool(
        httpClientFactory.CreateClient());

registry.Register(
    decliningPerformanceTool);


//// ---------------------------------------------------------
//// 10. Create and register OutstandingFeesTool
//// ---------------------------------------------------------

var outstandingFeesTool =
    new OutstandingFeesTool(
        httpClientFactory.CreateClient());

registry.Register(
    outstandingFeesTool);


//// ---------------------------------------------------------
//// 11. Create and register OverdueFeesTool
//// ---------------------------------------------------------

var overdueFeesTool =
new OverdueFeesTool(
    httpClientFactory.CreateClient());

registry.Register(
    overdueFeesTool);


//// ---------------------------------------------------------
//// 12. Create and register FeeSummaryTool
//// ---------------------------------------------------------

var feeSummaryTool =
    new FeeSummaryTool(
        httpClientFactory.CreateClient());

registry.Register(
    feeSummaryTool);



//// ---------------------------------------------------------
//// 13. Create and register AdmissionSummaryTool
//// ---------------------------------------------------------

var admissionSummaryTool =
    new AdmissionSummaryTool(
        httpClientFactory.CreateClient());

registry.Register(
    admissionSummaryTool);


//// ---------------------------------------------------------
//// 14. Create and register AdmissionsByStatusTool
//// ---------------------------------------------------------

var admissionsByStatusTool =
    new AdmissionsByStatusTool(
        httpClientFactory.CreateClient());

registry.Register(
    admissionsByStatusTool);


//// ---------------------------------------------------------
//// 15. Create and register AdmissionDetailsTool
//// ---------------------------------------------------------

var admissionDetailsTool =
    new AdmissionDetailsTool(
        httpClientFactory.CreateClient());

registry.Register(
    admissionDetailsTool);


// ---------------------------------------------------------
// 16. Create and register DocumentCatalogTool
// ---------------------------------------------------------

var documentCatalogTool =
    new DocumentCatalogTool(
        httpClientFactory.CreateClient());

registry.Register(
    documentCatalogTool);


// ---------------------------------------------------------
// 17. Create and register ReadDocumentTool
// ---------------------------------------------------------

var readDocumentTool =
    new ReadDocumentTool(
        httpClientFactory.CreateClient());

registry.Register(
    readDocumentTool);



//// ---------------------------------------------------------
//// 18. Create AI Orchestrator
//// ---------------------------------------------------------

var orchestrator =
    new AIOrchestrator(
        client,
        registry);


//// ---------------------------------------------------------
//// 19. Read user JWT
//// ---------------------------------------------------------
////
//// Get this from:
////
//// POST /api/Auth/login
////
//// Do NOT include "Bearer ".
//// ---------------------------------------------------------

Console.Write(
    "Enter JWT access token: ");

var accessToken =
    Console.ReadLine() ?? "";


//// ---------------------------------------------------------
//// 20. Send request to AI Agent
//// ---------------------------------------------------------

var result =
    await orchestrator.ProcessAsync(
            "Show me document with ID 2.",
        accessToken);


//// ---------------------------------------------------------
//// 21. Display final AI response
//// ---------------------------------------------------------

Console.WriteLine();

Console.WriteLine(
    "==========================================");

Console.WriteLine(
    "AI Response");

Console.WriteLine(
    "==========================================");

Console.WriteLine(result);

Console.WriteLine();








