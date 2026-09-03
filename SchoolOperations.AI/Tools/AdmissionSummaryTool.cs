using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class AdmissionSummaryTool : ITool
{
    private readonly HttpClient _httpClient;

    public AdmissionSummaryTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public string Name =>
        "GetAdmissionSummary";


    public string Description =>
        "Gets a summary of admission applications, including total " +
        "applications and the number of pending, approved, rejected, " +
        "and waitlisted applications.";


    // This tool does not require any arguments
    public Dictionary<string, Schema> Parameters =>
        new();


    // =========================================================
    // Execute Admission Summary Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. No arguments are required
        // ---------------------------------------------------------


        // ---------------------------------------------------------
        // 2. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access admission summary data.");
        }


        // ---------------------------------------------------------
        // 3. Attach JWT to HTTP request
        // ---------------------------------------------------------

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                context.AccessToken);


        // ---------------------------------------------------------
        // 4. Build backend URL
        // ---------------------------------------------------------

        var url =
            "https://localhost:7003/api/admissions/summary";


        // ---------------------------------------------------------
        // 5. Call backend API
        // ---------------------------------------------------------

        HttpResponseMessage response;

        try
        {
            response =
                await _httpClient.GetAsync(url);
        }
        catch (HttpRequestException)
        {
            return ToolResult.Fail(
                "BackendUnavailable",
                "The admissions backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 6. Handle HTTP errors
        // ---------------------------------------------------------

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:

                    return ToolResult.Fail(
                        "Unauthorized",
                        "Authentication is required to access admission summary data.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access admission summary data.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The admission summary request was invalid."
                            : error);


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The admission summary resource was not found.");


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The admissions backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 7. Read successful JSON response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 8. Deserialize backend response
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<AdmissionSummaryResultDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        // ---------------------------------------------------------
        // 9. Return successful ToolResult
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The admissions backend returned an empty or invalid response.");
        }

        return ToolResult.Ok(
            result);
    }
}


// =============================================================
// Represents the successful admission summary data
// =============================================================

public class AdmissionSummaryResultDto
{
    public int TotalApplications { get; set; }

    public int PendingApplications { get; set; }

    public int ApprovedApplications { get; set; }

    public int RejectedApplications { get; set; }

    public int WaitlistedApplications { get; set; }
}