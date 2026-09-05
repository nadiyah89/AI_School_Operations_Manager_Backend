using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class GetMyMeetingsTool : ITool
{
    private readonly HttpClient _httpClient;

    public GetMyMeetingsTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetMyMeetings";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Gets active meetings for the currently authenticated Teacher " +
        "or Parent. Optionally filters meetings by status or calendar date.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["status"] = new Schema
            {
                Type = "string",
                Description =
                    "Optional meeting status to filter by. " +
                    "Allowed values are Scheduled, Completed, or Cancelled."
            },

            ["date"] = new Schema
            {
                Type = "string",
                Description =
                    "Optional calendar date to filter meetings by, " +
                    "using YYYY-MM-DD format."
            }
        };


    // =========================================================
    // Execute Get My Meetings Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access meetings.");
        }


        // ---------------------------------------------------------
        // 2. Read optional status argument
        // ---------------------------------------------------------

        string? status = null;

        if (arguments.TryGetValue(
                "status",
                out var statusValue) &&
            statusValue != null)
        {
            status = statusValue.ToString();

            if (string.IsNullOrWhiteSpace(status))
            {
                status = null;
            }
        }


        // ---------------------------------------------------------
        // 3. Validate status when provided
        // ---------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(status) &&
            status != "Scheduled" &&
            status != "Completed" &&
            status != "Cancelled")
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "Meeting status must be Scheduled, Completed, or Cancelled.");
        }


        // ---------------------------------------------------------
        // 4. Read optional date argument
        // ---------------------------------------------------------

        string? date = null;

        if (arguments.TryGetValue(
                "date",
                out var dateValue) &&
            dateValue != null)
        {
            date = dateValue.ToString();

            if (string.IsNullOrWhiteSpace(date))
            {
                date = null;
            }
        }


        // ---------------------------------------------------------
        // 5. Validate date when provided
        // ---------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!DateTime.TryParseExact(
                    date,
                    "yyyy-MM-dd",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out _))
            {
                return ToolResult.Fail(
                    "InvalidArgument",
                    "Meeting date must use YYYY-MM-DD format.");
            }
        }


        // ---------------------------------------------------------
        // 6. Attach JWT
        // ---------------------------------------------------------

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                context.AccessToken);


        // ---------------------------------------------------------
        // 7. Build backend URL
        // ---------------------------------------------------------

        var url =
            "https://localhost:7003/api/meetings/my";

        var queryParameters =
            new List<string>();


        if (!string.IsNullOrWhiteSpace(status))
        {
            queryParameters.Add(
                $"status={Uri.EscapeDataString(status)}");
        }


        if (!string.IsNullOrWhiteSpace(date))
        {
            queryParameters.Add(
                $"date={Uri.EscapeDataString(date)}");
        }


        if (queryParameters.Count > 0)
        {
            url +=
                "?" +
                string.Join(
                    "&",
                    queryParameters);
        }


        // ---------------------------------------------------------
        // 8. Call backend API
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
                "The meetings backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 9. Handle backend errors
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
                        "Authentication is required to access meetings.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access personal meetings.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The meetings request was invalid."
                            : error);


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The meetings backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 10. Read successful JSON response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 11. Deserialize backend response
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<
                List<MeetingSummaryResultDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        // ---------------------------------------------------------
        // 12. Validate response
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The meetings backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 13. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Represents a meeting summary returned by the backend
// =============================================================

public class MeetingSummaryResultDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; }
        = string.Empty;

    public int TeacherId { get; set; }

    public string TeacherName { get; set; }
        = string.Empty;

    public DateTime MeetingDate { get; set; }

    public string Purpose { get; set; }
        = string.Empty;

    public string Status { get; set; }
        = string.Empty;
}