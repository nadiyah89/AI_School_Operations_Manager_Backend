using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class GetStudentDetailsTool : ITool
{
    private readonly HttpClient _httpClient;

    public GetStudentDetailsTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetStudentDetails";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Gets the details of a specific student using their student ID. " +
        "A valid student ID is required.";


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
                    "Required ID of the student whose details should be retrieved."
            }
        };


    // =========================================================
    // Execute Get Student Details Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Validate required ID argument
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "id",
                out var idValue) ||
            idValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A student ID is required.");
        }


        // ---------------------------------------------------------
        // 2. Parse and validate student ID
        // ---------------------------------------------------------

        int studentId;

        if (idValue is JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Number ||
                !element.TryGetInt32(out studentId))
            {
                return ToolResult.Fail(
                    "InvalidArgument",
                    "The student ID must be a valid integer.");
            }
        }
        else
        {
            if (!int.TryParse(
                    idValue.ToString(),
                    out studentId))
            {
                return ToolResult.Fail(
                    "InvalidArgument",
                    "The student ID must be a valid integer.");
            }
        }


        if (studentId <= 0)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The student ID must be greater than zero.");
        }


        // ---------------------------------------------------------
        // 3. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access student details.");
        }


        // ---------------------------------------------------------
        // 4. Build backend URL
        // ---------------------------------------------------------

        var url =
            $"api/students/{studentId}";


        // ---------------------------------------------------------
        // 5. Call backend API
        // ---------------------------------------------------------

        HttpResponseMessage response;

        try
        {
            using var request = ToolHttpHelper.CreateRequest(
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
                "The students backend could not be reached.");
        }


        using (response)
        {
            // ---------------------------------------------------------
            // 6. Handle backend errors
            // ---------------------------------------------------------

            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:

                        return ToolResult.Fail(
                            "Unauthorized",
                            "Authentication is required to access student details.");


                    case HttpStatusCode.Forbidden:

                        return ToolResult.Fail(
                            "Forbidden",
                            "The authenticated user does not have permission " +
                            "to access this student.");


                    case HttpStatusCode.NotFound:

                        return ToolResult.Fail(
                            "NotFound",
                            "The requested student was not found.");


                    default:

                        return ToolResult.Fail(
                            $"HttpError_{(int)response.StatusCode}",
                            "The students backend returned an unexpected error.");
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
                JsonSerializer.Deserialize<StudentDetailsResultDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


            // ---------------------------------------------------------
            // 9. Validate response
            // ---------------------------------------------------------

            if (result == null)
            {
                return ToolResult.Fail(
                    "InvalidResponse",
                    "The students backend returned an empty or invalid response.");
            }


            // ---------------------------------------------------------
            // 10. Return successful ToolResult
            // ---------------------------------------------------------

            return ToolResult.Ok(result);
        }
    }
}


// =============================================================
// Represents a student returned by the details endpoint
// =============================================================

public class StudentDetailsResultDto
{
    public int Id { get; set; }

    public string FirstName { get; set; }
        = string.Empty;

    public string LastName { get; set; }
        = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public bool IsActive { get; set; }
}