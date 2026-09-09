using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class SearchTeachersTool : ITool
{
    private readonly HttpClient _httpClient;

    public SearchTeachersTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "SearchTeachers";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Searches for active teachers by first name or last name. " +
        "A non-empty teacher name search query is required.";


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
                    "Required teacher name to search for. " +
                    "Can be a first name, last name, or part of a name."
            }
        };


    // =========================================================
    // Execute Search Teachers Tool
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
                "A teacher name search query is required.");
        }


        string? query;

        if (queryValue is JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                return ToolResult.Fail(
                    "InvalidArgument",
                    "The teacher search query must be a string.");
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
                "The teacher search query cannot be empty.");
        }

        query = query.Trim();


        // ---------------------------------------------------------
        // 3. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to search for teachers.");
        }


        // ---------------------------------------------------------
        // 4. Build backend URL
        // ---------------------------------------------------------

        var url =
            "api/teachers" +
            $"?search={Uri.EscapeDataString(query)}";


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
                "The teachers backend could not be reached.");
        }


        using (response)
        {
            // ---------------------------------------------------------
            // 6. Handle backend errors
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
                            "Authentication is required to search for teachers.");


                    case HttpStatusCode.Forbidden:

                        return ToolResult.Fail(
                            "Forbidden",
                            "The authenticated user does not have permission " +
                            "to search for teachers.");


                    case HttpStatusCode.BadRequest:

                        return ToolResult.Fail(
                            "BadRequest",
                            string.IsNullOrWhiteSpace(error)
                                ? "The teacher search request was invalid."
                                : error);


                    default:

                        return ToolResult.Fail(
                            $"HttpError_{(int)response.StatusCode}",
                            "The teachers backend returned an unexpected error.");
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
                    List<TeacherSearchResultDto>>(
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
                    "The teachers backend returned an empty or invalid response.");
            }


            // ---------------------------------------------------------
            // 10. Return successful ToolResult
            // ---------------------------------------------------------

            return ToolResult.Ok(result);
        }
    }
}


// =============================================================
// Represents a teacher returned by the search endpoint
// =============================================================

public class TeacherSearchResultDto
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

    public bool IsActive { get; set; }
}