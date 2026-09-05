using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class GetParentsByStudentTool : ITool
{
    private readonly HttpClient _httpClient;

    public GetParentsByStudentTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetParentsByStudent";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Gets active parent and emergency contact information for a " +
        "specific student using the student's ID.";


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
                    "Required ID of the student whose active parents " +
                    "or emergency contacts should be retrieved."
            }
        };


    // =========================================================
    // Execute Get Parents By Student Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Validate required studentId argument
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
        // 2. Parse student ID
        // ---------------------------------------------------------

        int studentId;

        if (studentIdValue is JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Number ||
                !element.TryGetInt32(out studentId))
            {
                return ToolResult.Fail(
                    "InvalidArgument",
                    "The student ID must be a valid integer.");
            }
        }
        else if (!int.TryParse(
                     studentIdValue.ToString(),
                     out studentId))
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

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access parent information.");
        }


        // ---------------------------------------------------------
        // 5. Attach JWT
        // ---------------------------------------------------------

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                context.AccessToken);


        // ---------------------------------------------------------
        // 6. Build backend URL
        // ---------------------------------------------------------

        var url =
            "https://localhost:7003/api/parents/student/" +
            studentId;


        // ---------------------------------------------------------
        // 7. Call backend API
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
                "The parents backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 8. Handle backend errors
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
                        "Authentication is required to access parent information.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access parent information.");


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested student was not found.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The parent information request was invalid."
                            : error);


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The parents backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 9. Read successful JSON response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 10. Deserialize backend response
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<
                List<ParentByStudentResultDto>>(
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
                "The parents backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 12. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Represents a parent returned for a specific student
// =============================================================

public class ParentByStudentResultDto
{
    public int Id { get; set; }

    public string FirstName { get; set; }
        = string.Empty;

    public string LastName { get; set; }
        = string.Empty;

    public string PhoneNumber { get; set; }
        = string.Empty;

    public string Email { get; set; }
        = string.Empty;

    public string Relationship { get; set; }
        = string.Empty;

    public int StudentId { get; set; }

    public bool IsActive { get; set; }

    public ParentByStudentStudentDto? Student { get; set; }
}


// =============================================================
// Represents the included student information
// =============================================================

public class ParentByStudentStudentDto
{
    public int Id { get; set; }

    public string FirstName { get; set; }
        = string.Empty;

    public string LastName { get; set; }
        = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public bool IsActive { get; set; }
}