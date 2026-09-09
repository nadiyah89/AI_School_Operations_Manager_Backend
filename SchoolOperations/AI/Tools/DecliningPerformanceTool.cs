using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class DecliningPerformanceTool : ITool
{
    private readonly HttpClient _httpClient;

    public DecliningPerformanceTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Identity
    // =========================================================

    public string Name =>
        "GetStudentsWithDecliningPerformance";


    public string Description =>
        "Finds students whose latest academic performance " +
        "percentage is lower than their previous examination " +
        "percentage. Use this tool only when the user explicitly " +
        "asks about declining, dropping, or worsening performance. " +
        "Optionally filters results by subject.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["subject"] = new Schema
            {
                Type = "string",

                Description =
                    "Optional subject to filter declining academic " +
                    "performance. For example, Math."
            }
        };


    // =========================================================
    // Execute Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Read optional subject
        // ---------------------------------------------------------

        string? subject = null;

        if (arguments.TryGetValue(
                "subject",
                out var subjectValue))
        {
            if (subjectValue is not JsonElement subjectElement ||
                subjectElement.ValueKind != JsonValueKind.String)
            {
                return ToolResult.Fail(
                    "InvalidArgument",
                    "The 'subject' argument must be a string.");
            }

            subject =
                subjectElement.GetString();
        }


        // ---------------------------------------------------------
        // 2. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access academic performance data.");
        }


        // ---------------------------------------------------------
        // 3. Build backend URL
        // ---------------------------------------------------------

        var url =
            "api/academicperformance/declining";

        // Add subject only when provided
        if (!string.IsNullOrWhiteSpace(subject))
        {
            var encodedSubject =
                Uri.EscapeDataString(subject);

            url +=
                $"?subject={encodedSubject}";
        }


        // ---------------------------------------------------------
        // 4. Create authenticated HTTP request
        // ---------------------------------------------------------

        using var request =
            ToolHttpHelper.CreateRequest(
                HttpMethod.Get,
                url,
                context.AccessToken);


        // ---------------------------------------------------------
        // 5. Call backend API
        // ---------------------------------------------------------

        HttpResponseMessage response;

        try
        {
            response =
                await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            return ToolResult.Fail(
                "BackendUnavailable",
                "The academic performance backend could not be reached.");
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
                        "Authentication is required to access academic performance data.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access academic performance data.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The declining performance request was invalid."
                            : error);


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested academic performance resource was not found.");


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The academic performance backend returned an unexpected error.");
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
            JsonSerializer.Deserialize<
                List<DecliningPerformanceSummaryDto>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


        // ---------------------------------------------------------
        // 9. Return ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(
            result ?? []);
    }
}


// =============================================================
// Represents successful declining performance data
// =============================================================

public class DecliningPerformanceSummaryDto
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;


    public string PreviousExamName { get; set; } = string.Empty;

    public DateTime PreviousExamDate { get; set; }

    public decimal PreviousPercentage { get; set; }


    public string LatestExamName { get; set; } = string.Empty;

    public DateTime LatestExamDate { get; set; }

    public decimal LatestPercentage { get; set; }


    public decimal PercentageChange { get; set; }
}