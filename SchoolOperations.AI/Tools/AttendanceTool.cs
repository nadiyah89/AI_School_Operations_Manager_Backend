using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class AttendanceTool : ITool
{
    private readonly HttpClient _httpClient;

    public AttendanceTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Name =>
        "GetStudentsBelowAttendanceThreshold";

    public string Description =>
        "Finds students whose attendance percentage " +
        "is below the specified threshold.";

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["threshold"] = new Schema
            {
                Type = "number",

                Description =
                    "Attendance percentage threshold. " +
                    "For example, 75 means students below 75%."
            }
        };


    // =========================================================
    // Execute Attendance Tool
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
        // 2. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access attendance data.");
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
            $"https://localhost:7003/api/attendance/summary" +
            $"?threshold={threshold}";


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
                "The attendance backend could not be reached.");
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
                // -------------------------------------------------
                // 401 Unauthorized
                // -------------------------------------------------

                case HttpStatusCode.Unauthorized:

                    return ToolResult.Fail(
                        "Unauthorized",
                        "Authentication is required to access attendance data.");


                // -------------------------------------------------
                // 403 Forbidden
                // -------------------------------------------------

                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access attendance data.");


                // -------------------------------------------------
                // 400 Bad Request
                // -------------------------------------------------

                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The attendance request was invalid."
                            : error);


                // -------------------------------------------------
                // 404 Not Found
                // -------------------------------------------------

                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested attendance resource was not found.");


                // -------------------------------------------------
                // Other HTTP errors
                // -------------------------------------------------

                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The attendance backend returned an unexpected error.");
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
                List<AttendanceSummaryDto>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


        // ---------------------------------------------------------
        // 9. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(
            result ?? []);
    }
}


// =============================================================
// Represents the successful attendance data
// =============================================================

public class AttendanceSummaryDto
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int TotalDays { get; set; }

    public int PresentDays { get; set; }

    public int AbsentDays { get; set; }

    public double AttendancePercentage { get; set; }
}