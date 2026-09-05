using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class SearchStudentsTool : ITool
{
    private readonly HttpClient _httpClient;

    public SearchStudentsTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "SearchStudents";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Searches for active students by first name or last name. " +
        "A non-empty student name search query is required.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["query"] = new Schema
            {
                Type = "string",
                Description =
                    "Required student name to search for. " +
                    "Can be a first name, last name, or part of a name."
            }
        };


    // =========================================================
    // Execute Search Students Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Validate required query argument
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "query",
                out var queryValue) ||
            queryValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A student name search query is required.");
        }


        string? query;

        if (queryValue is JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                return ToolResult.Fail(
                    "InvalidArgument",
                    "The student search query must be a string.");
            }

            query = element.GetString();
        }
        else
        {
            query = queryValue.ToString();
        }


        // ---------------------------------------------------------
        // 2. Validate query value
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(query))
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The student search query cannot be empty.");
        }

        query = query.Trim();


        // ---------------------------------------------------------
        // 3. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to search for students.");
        }


        // ---------------------------------------------------------
        // 4. Attach JWT
        // ---------------------------------------------------------

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                context.AccessToken);


        // ---------------------------------------------------------
        // 5. Build backend URL
        // ---------------------------------------------------------

        var url =
            "https://localhost:7003/api/students" +
            $"?search={Uri.EscapeDataString(query)}";


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
                "The students backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 7. Handle backend errors
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
                        "Authentication is required to search for students.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to search for students.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The student search request was invalid."
                            : error);


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The students backend returned an unexpected error.");
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
                List<StudentSearchResultDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        // ---------------------------------------------------------
        // 10. Validate response
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The students backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 11. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Represents a student returned by the search endpoint
// =============================================================

public class StudentSearchResultDto
{
    public int Id { get; set; }

    public string FirstName { get; set; }
        = string.Empty;

    public string LastName { get; set; }
        = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public bool IsActive { get; set; }
}