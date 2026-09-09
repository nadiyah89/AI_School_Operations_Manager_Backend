using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class GetStudentNotificationsTool : ITool
{
    private readonly HttpClient _httpClient;

    public GetStudentNotificationsTool(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetStudentNotifications";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Gets active notification records for a specific student using the student's ID. " +
        "Use this after identifying the student when the user asks about that student's notification history.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["studentId"] = new Schema
            {
                Type = "integer",
                Description =
                    "The ID of the student whose notification records should be retrieved."
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
        // 1. Validate student ID argument
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "studentId",
                out var studentIdValue) ||
            studentIdValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A student ID is required.");
        }


        // ---------------------------------------------------------
        // 2. Convert Gemini argument to integer
        // ---------------------------------------------------------

        int studentId;

        try
        {
            if (studentIdValue is JsonElement element)
            {
                studentId =
                    element.GetInt32();
            }
            else
            {
                studentId =
                    Convert.ToInt32(studentIdValue);
            }
        }
        catch
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The student ID must be a valid integer.");
        }


        // ---------------------------------------------------------
        // 3. Validate student ID value
        // ---------------------------------------------------------

        if (studentId <= 0)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The student ID must be greater than zero.");
        }


        // ---------------------------------------------------------
        // 4. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access notifications.");
        }


        // ---------------------------------------------------------
        // 5. Build backend URL
        // ---------------------------------------------------------

        var url =
            $"api/notifications/student/{studentId}";


        // ---------------------------------------------------------
        // 6. Call backend API
        // ---------------------------------------------------------

        HttpResponseMessage response;

        try
        {
            using var request =
                ToolHttpHelper.CreateRequest(
                    HttpMethod.Get,
                    url,
                    context.AccessToken);

            response =
                await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            return ToolResult.Fail(
                "BackendUnavailable",
                "The notifications backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 7. Handle backend errors
        // ---------------------------------------------------------

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:

                    return ToolResult.Fail(
                        "Unauthorized",
                        "Authentication is required to access notifications.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access these notifications.");


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested student does not exist or is not available.");


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The notifications backend returned an unexpected error.");
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

        List<StudentNotificationResultDto>? result;

        try
        {
            result =
                JsonSerializer.Deserialize<
                    List<StudentNotificationResultDto>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }
        catch (JsonException)
        {
            return ToolResult.Fail(
                "InvalidJson",
                "The backend returned an unexpected data format.");
        }


        // ---------------------------------------------------------
        // 10. Validate response
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The notifications backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 11. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Represents a notification returned for a student
// =============================================================

public class StudentNotificationResultDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int ParentId { get; set; }

    public string NotificationType { get; set; }
        = string.Empty;

    public string Message { get; set; }
        = string.Empty;

    public string Channel { get; set; }
        = string.Empty;

    public string Status { get; set; }
        = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }
}