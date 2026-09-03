using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class AdmissionDetailsTool : ITool
{
    private readonly HttpClient _httpClient;

    public AdmissionDetailsTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetAdmissionDetails";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Gets the complete details of a specific admission application " +
        "using its admission application ID.";


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
                    "The ID of the admission application."
            }
        };


    // =========================================================
    // Execute Admission Details Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Validate admission ID argument
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "id",
                out var idValue) ||
            idValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "An admission application ID is required.");
        }


        // ---------------------------------------------------------
        // 2. Convert Gemini argument to integer
        // ---------------------------------------------------------

        int admissionId;

        try
        {
            if (idValue is JsonElement element)
            {
                admissionId =
                    element.GetInt32();
            }
            else
            {
                admissionId =
                    Convert.ToInt32(idValue);
            }
        }
        catch
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The admission application ID must be a valid integer.");
        }


        // ---------------------------------------------------------
        // 3. Validate admission ID value
        // ---------------------------------------------------------

        if (admissionId <= 0)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The admission application ID must be greater than zero.");
        }


        // ---------------------------------------------------------
        // 4. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access admission data.");
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
            $"https://localhost:7003/api/admissions/{admissionId}";


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
                "The admissions backend could not be reached.");
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
                        "Authentication is required to access admission data.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access admission data.");


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The admission application does not exist.");


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The admissions backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 9. Read successful response
        // ---------------------------------------------------------

        var json =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 10. Deserialize admission data
        // ---------------------------------------------------------

        var result =
            JsonSerializer.Deserialize<AdmissionDetailsResultDto>(
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
                "The admissions backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 12. Return successful result
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Admission Details Result DTO
// =============================================================

public class AdmissionDetailsResultDto
{
    public int Id { get; set; }

    public string ApplicantFirstName { get; set; } =
        string.Empty;

    public string ApplicantLastName { get; set; } =
        string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string ApplyingForClass { get; set; } =
        string.Empty;

    public string ParentName { get; set; } =
        string.Empty;

    public string ParentPhoneNumber { get; set; } =
        string.Empty;

    public string ParentEmail { get; set; } =
        string.Empty;

    public DateTime ApplicationDate { get; set; }

    public string Status { get; set; } =
        string.Empty;

    public int? StudentId { get; set; }

    public int? ParentId { get; set; }
}