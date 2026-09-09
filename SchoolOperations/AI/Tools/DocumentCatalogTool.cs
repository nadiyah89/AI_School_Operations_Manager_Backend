using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class DocumentCatalogTool : ITool
{
    private readonly HttpClient _httpClient;

    public DocumentCatalogTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public string Name =>
        "GetDocumentCatalog";


    public string Description =>
        "Gets a lightweight catalog of available active school documents. " +
        "Optionally filters documents by category. " +
        "Use this tool to find relevant documents before reading a specific document.";


    // Category is optional
    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["category"] = new Schema
            {
                Type = "string",
                Description =
                    "Optional document category to filter by, such as " +
                    "Policy, Admission, Academic, or Attendance."
            }
        };


    // =========================================================
    // Execute Document Catalog Tool
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
                "An access token is required to access documents.");
        }


        // ---------------------------------------------------------
        // 2. Read optional category argument
        // ---------------------------------------------------------

        string? category = null;

        if (arguments.TryGetValue(
                "category",
                out var categoryValue) &&
            categoryValue != null)
        {
            category = categoryValue.ToString();

            if (string.IsNullOrWhiteSpace(category))
            {
                category = null;
            }
        }


        // ---------------------------------------------------------
        // 3. Build backend URL
        // ---------------------------------------------------------

        var url =
            "api/documents/catalog";

        if (!string.IsNullOrWhiteSpace(category))
        {
            url +=
                $"?category={Uri.EscapeDataString(category)}";
        }


        // ---------------------------------------------------------
        // 4. Call backend API
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
        // 5. Handle HTTP errors
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
                        "Authentication is required to access documents.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access documents.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The document catalog request was invalid."
                            : error);


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The documents backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 6. Read successful JSON response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 7. Deserialize backend response
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<
                List<DocumentSummaryResultDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        // ---------------------------------------------------------
        // 8. Validate response
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The documents backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 9. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Represents a document summary returned by the backend
// =============================================================

public class DocumentSummaryResultDto
{
    public int Id { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Category { get; set; }
        = string.Empty;

    public DateTime UpdatedAt { get; set; }
}