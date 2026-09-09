using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class AdmissionsByStatusTool : ITool
{
    private readonly HttpClient _httpClient;

    public AdmissionsByStatusTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public string Name =>
        "GetAdmissionsByStatus";


    public string Description =>
        "Gets admission applications filtered by their status. " +
        "Valid statuses are Pending, Approved, Rejected, and Waitlisted.";


    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["status"] = new Schema
            {
                Type = "string",
                Description =
                    "The admission status to filter by. Valid values are " +
                    "Pending, Approved, Rejected, or Waitlisted."
            }
        };


    // =========================================================
    // Execute Admissions By Status Tool
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
                "An access token is required to access admission data.");
        }


        // ---------------------------------------------------------
        // 2. Read status argument
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "status",
                out var statusValue) ||
            statusValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "An admission status is required.");
        }


        string? status;

        if (statusValue is JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                return ToolResult.Fail(
                    "InvalidStatus",
                    "Invalid admission status. Valid statuses are: " +
                    "Pending, Approved, Rejected, Waitlisted.");
            }

            status = element.GetString();
        }
        else
        {
            status = statusValue.ToString();
        }


        // ---------------------------------------------------------
        // 3. Validate status
        // ---------------------------------------------------------

        var validStatuses = new[]
        {
            "Pending",
            "Approved",
            "Rejected",
            "Waitlisted"
        };

        if (string.IsNullOrWhiteSpace(status) ||
            !validStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
        {
            return ToolResult.Fail(
                "InvalidStatus",
                "Invalid admission status. Valid statuses are: " +
                "Pending, Approved, Rejected, Waitlisted.");
        }


        // ---------------------------------------------------------
        // 4. Normalize status
        // ---------------------------------------------------------

        status =
            validStatuses.First(
                s => s.Equals(
                    status,
                    StringComparison.OrdinalIgnoreCase));


        // ---------------------------------------------------------
        // 5. Build relative backend URL
        // ---------------------------------------------------------

        var url =
            $"api/admissions?status={Uri.EscapeDataString(status)}";


        // ---------------------------------------------------------
        // 6. Create authenticated HTTP request
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
                "The admissions backend could not be reached.");
        }


        using (response)
        {
            // ---------------------------------------------------------
            // 8. Handle HTTP errors
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
                            "Authentication is required to access admission data.");


                    case HttpStatusCode.Forbidden:

                        return ToolResult.Fail(
                            "Forbidden",
                            "The authenticated user does not have permission " +
                            "to access admission data.");


                    case HttpStatusCode.BadRequest:

                        return ToolResult.Fail(
                            "BadRequest",
                            string.IsNullOrWhiteSpace(error)
                                ? "The admission request was invalid."
                                : error);


                    case HttpStatusCode.NotFound:

                        return ToolResult.Fail(
                            "NotFound",
                            "The requested admission resource was not found.");


                    default:

                        return ToolResult.Fail(
                            $"HttpError_{(int)response.StatusCode}",
                            "The admissions backend returned an unexpected error.");
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
                    List<AdmissionApplicationResultDto>>(
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
            // 12. Return successful ToolResult
            // ---------------------------------------------------------

            return ToolResult.Ok(result);
        }
    }
}


// =============================================================
// Represents an admission application returned by the backend
// =============================================================

public class AdmissionApplicationResultDto
{
    public int Id { get; set; }

    public string ApplicantFirstName { get; set; }
        = string.Empty;

    public string ApplicantLastName { get; set; }
        = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string ApplyingForClass { get; set; }
        = string.Empty;

    public string ParentName { get; set; }
        = string.Empty;

    public string ParentPhoneNumber { get; set; }
        = string.Empty;

    public string ParentEmail { get; set; }
        = string.Empty;

    public DateTime ApplicationDate { get; set; }

    public string Status { get; set; }
        = string.Empty;

    public int? StudentId { get; set; }

    public int? ParentId { get; set; }
}