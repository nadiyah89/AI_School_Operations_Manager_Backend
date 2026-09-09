using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class GetMeetingDetailsTool : ITool
{
    private readonly HttpClient _httpClient;

    public GetMeetingDetailsTool(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetMeetingDetails";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Gets the complete details of a specific meeting using its meeting ID. " +
        "Use this after identifying a relevant meeting from GetMyMeetings when " +
        "the user needs detailed information such as notes.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["id"] = new Schema
            {
                Type = "integer",
                Description =
                    "The ID of the meeting to retrieve."
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
        // 1. Validate meeting ID argument
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "id",
                out var idValue) ||
            idValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A meeting ID is required.");
        }


        // ---------------------------------------------------------
        // 2. Convert Gemini argument to integer
        // ---------------------------------------------------------

        int meetingId;

        try
        {
            if (idValue is JsonElement element)
            {
                meetingId =
                    element.GetInt32();
            }
            else
            {
                meetingId =
                    Convert.ToInt32(idValue);
            }
        }
        catch
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The meeting ID must be a valid integer.");
        }


        // ---------------------------------------------------------
        // 3. Validate meeting ID value
        // ---------------------------------------------------------

        if (meetingId <= 0)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The meeting ID must be greater than zero.");
        }


        // ---------------------------------------------------------
        // 4. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access meetings.");
        }


        // ---------------------------------------------------------
        // 5. Build backend URL
        // ---------------------------------------------------------

        var url =
            $"api/meetings/{meetingId}";


        // ---------------------------------------------------------
        // 6. Create isolated authenticated request
        // ---------------------------------------------------------

        using var request =
            ToolHttpHelper.CreateRequest(
                HttpMethod.Get,
                url,
                context.AccessToken);


        // ---------------------------------------------------------
        // 7. Call backend API
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
                "The meetings backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 8. Handle backend errors
        // ---------------------------------------------------------

        if (!response.IsSuccessStatusCode)
        {
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
                        "to access this meeting.");


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested meeting does not exist or is not available.");


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The meetings backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 9. Read successful response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 10. Deserialize meeting data
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<
                MeetingDetailsResultDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        // ---------------------------------------------------------
        // 11. Validate response
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The meetings backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 12. Return successful result
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Meeting Details Result DTO
// =============================================================

public class MeetingDetailsResultDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int TeacherId { get; set; }

    public DateTime MeetingDate { get; set; }

    public string Purpose { get; set; }
        = string.Empty;

    public string Status { get; set; }
        = string.Empty;

    public string? Notes { get; set; }

    public bool IsActive { get; set; }
}