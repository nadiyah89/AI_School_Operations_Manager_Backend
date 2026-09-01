using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class PoorPerformanceTool : ITool
{
    private readonly HttpClient _httpClient;

    public PoorPerformanceTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Name =>
        "GetStudentsBelowPerformanceThreshold";

    public string Description =>
     "Finds students whose latest academic performance " +
     "percentage is below a specified threshold. " +
     "Use this tool only when the user explicitly asks about " +
     "performance below a percentage or threshold. " +
     "Optionally filters results by subject.";

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["threshold"] = new Schema
            {
                Type = "number",

                Description =
                    "Academic performance percentage threshold. " +
                    "For example, 60 means students below 60%."
            },

            ["subject"] = new Schema
            {
                Type = "string",

                Description =
                    "Optional subject to filter academic performance. " +
                    "For example, Math."
            }
        };


    // =========================================================
    // Execute Poor Performance Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Read and validate threshold
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "threshold",
                out var thresholdValue))
        {
            return ToolResult.Fail(
                "MissingArgument",
                "The 'threshold' argument is required.");
        }


        if (thresholdValue is not JsonElement thresholdElement)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The 'threshold' argument must be a number.");
        }


        if (thresholdElement.ValueKind != JsonValueKind.Number)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The 'threshold' argument must be a number.");
        }


        var threshold =
            thresholdElement.GetDouble();


        // ---------------------------------------------------------
        // 2. Read optional subject
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
        // 3. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access academic performance data.");
        }


        // ---------------------------------------------------------
        // 4. Attach JWT to HTTP request
        // ---------------------------------------------------------

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                context.AccessToken);


        // ---------------------------------------------------------
        // 5. Build backend URL
        // ---------------------------------------------------------

        var url =
            $"https://localhost:7003/api/academicperformance/poor" +
            $"?threshold={threshold}";

        // Add subject only when one is provided
        if (!string.IsNullOrWhiteSpace(subject))
        {
            var encodedSubject =
                Uri.EscapeDataString(subject);

            url +=
                $"&subject={encodedSubject}";
        }


        // ---------------------------------------------------------
        // 6. Call backend API
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
                "The academic performance backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 7. Handle HTTP errors
        // ---------------------------------------------------------

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                // -------------------------------------------------
                // 401 Unauthorized
                // -------------------------------------------------

                case HttpStatusCode.Unauthorized:

                    return ToolResult.Fail(
                        "Unauthorized",
                        "Authentication is required to access academic performance data.");


                // -------------------------------------------------
                // 403 Forbidden
                // -------------------------------------------------

                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access academic performance data.");


                // -------------------------------------------------
                // 400 Bad Request
                // -------------------------------------------------

                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The academic performance request was invalid."
                            : error);


                // -------------------------------------------------
                // 404 Not Found
                // -------------------------------------------------

                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested academic performance resource was not found.");


                // -------------------------------------------------
                // Other HTTP errors
                // -------------------------------------------------

                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The academic performance backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 8. Read successful JSON response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 9. Deserialize backend response
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<
                List<PoorPerformanceSummaryDto>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


        // ---------------------------------------------------------
        // 10. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(
            result ?? []);
    }
}


// =============================================================
// Represents the successful poor performance data
// =============================================================

public class PoorPerformanceSummaryDto
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string LatestExamName { get; set; } = string.Empty;

    public DateTime LatestExamDate { get; set; }

    public decimal LatestPercentage { get; set; }
}