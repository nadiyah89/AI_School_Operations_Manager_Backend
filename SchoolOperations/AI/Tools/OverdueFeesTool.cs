using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class OverdueFeesTool : ITool
{
    private readonly HttpClient _httpClient;

    public OverdueFeesTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetStudentsWithOverdueFees";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Finds active students who have active fee records " +
        "where the due date has passed and an outstanding balance remains.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    // This tool does not require any arguments
    public Dictionary<string, Schema> Parameters =>
        new();


    // =========================================================
    // Execute Overdue Fees Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. No arguments are required
        // ---------------------------------------------------------


        // ---------------------------------------------------------
        // 2. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to access fee data.");
        }


        // ---------------------------------------------------------
        // 3. Create authenticated HTTP request
        // ---------------------------------------------------------

        using var request =
            ToolHttpHelper.CreateRequest(
                HttpMethod.Get,
                "api/feerecords/overdue",
                context.AccessToken);


        // ---------------------------------------------------------
        // 4. Call backend API
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
                "The fee records backend could not be reached.");
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
                        "Authentication is required to access fee data.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access fee data.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The overdue fee request was invalid."
                            : error);


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested overdue fee resource was not found.");


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The fee records backend returned an unexpected error.");
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
                List<OverdueFeeResultDto>>(
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
                "The fee records backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 9. Return successful ToolResult
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Represents the successful overdue fee data
// =============================================================

public class OverdueFeeResultDto
{
    public int FeeRecordId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; }
        = string.Empty;

    public string FeeType { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal OutstandingAmount { get; set; }

    public DateTime DueDate { get; set; }

    public string Status { get; set; }
        = string.Empty;
}