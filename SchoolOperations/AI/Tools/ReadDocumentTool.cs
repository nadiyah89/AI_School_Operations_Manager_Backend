using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class ReadDocumentTool : ITool
{
    private readonly HttpClient _httpClient;

    public ReadDocumentTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "ReadDocument";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Reads the complete content and details of a specific school " +
        "document using its document ID.";


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
                    "The ID of the document to read."
            }
        };


    // =========================================================
    // Execute Read Document Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Validate document ID argument
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "id",
                out var idValue) ||
            idValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A document ID is required.");
        }


        // ---------------------------------------------------------
        // 2. Convert Gemini argument to integer
        // ---------------------------------------------------------

        int documentId;

        try
        {
            if (idValue is JsonElement element)
            {
                documentId =
                    element.GetInt32();
            }
            else
            {
                documentId =
                    Convert.ToInt32(idValue);
            }
        }
        catch
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The document ID must be a valid integer.");
        }


        // ---------------------------------------------------------
        // 3. Validate document ID value
        // ---------------------------------------------------------

        if (documentId <= 0)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The document ID must be greater than zero.");
        }


        // ---------------------------------------------------------
        // 4. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access documents.");
        }


        // ---------------------------------------------------------
        // 5. Build backend URL
        // ---------------------------------------------------------

        var url =
            $"api/documents/{documentId}";


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
                "The documents backend could not be reached.");
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
                        "Authentication is required to access documents.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access documents.");


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested document does not exist or is not available.");


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The documents backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 8. Read successful response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 9. Deserialize document data
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<DocumentDetailsResultDto>(
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
                "The documents backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 11. Return successful result
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Document Details Result DTO
// =============================================================

public class DocumentDetailsResultDto
{
    public int Id { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Category { get; set; }
        = string.Empty;

    public string Content { get; set; }
        = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; }
}