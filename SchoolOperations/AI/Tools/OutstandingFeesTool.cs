using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class OutstandingFeesTool : ITool
{
    private readonly HttpClient _httpClient;

    public OutstandingFeesTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "GetStudentsWithOutstandingFees";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Finds active students who have active fee records " +
        "with an outstanding balance.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    // This tool does not require any arguments
    public Dictionary<string, Schema> Parameters =>
        new();


    // =========================================================
    // Execute Outstanding Fees Tool
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
                "api/feerecords/outstanding",
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
                            ? "The fee records request was invalid."
                            : error);


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The requested fee records resource was not found.");


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
                List<OutstandingFeeResultDto>>(
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
// Represents the successful outstanding fee data
// =============================================================

public class OutstandingFeeResultDto
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