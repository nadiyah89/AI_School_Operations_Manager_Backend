using System.Net;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class FeeSummaryTool : ITool
{
    private readonly HttpClient _httpClient;

    public FeeSummaryTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public string Name =>
        "GetFeeRecordsSummary";


    public string Description =>
        "Gets a summary of the school's active fee records, " +
        "including total fees, paid amounts, outstanding amounts, " +
        "payment statuses, and overdue fee records.";


    // This tool does not require any arguments
    public Dictionary<string, Schema> Parameters =>
        new();


    // =========================================================
    // Execute Fee Summary Tool
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
                "An access token is required to access fee summary data.");
        }


        // ---------------------------------------------------------
        // 3. Build thread-safe authenticated request
        // ---------------------------------------------------------

        using var request =
            ToolHttpHelper.CreateRequest(
                HttpMethod.Get,
                "api/feerecords/summary",
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
                        "Authentication is required to access fee summary data.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to access fee summary data.");


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The fee summary request was invalid."
                            : error);


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        "The fee summary resource was not found.");


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
            JsonSerializer.Deserialize<FeeSummaryResultDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        // ---------------------------------------------------------
        // 8. Validate and return successful ToolResult
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The fee summary backend returned an empty or invalid response.");
        }

        return ToolResult.Ok(result);
    }
}


// =============================================================
// Represents the successful fee summary data
// =============================================================

public class FeeSummaryResultDto
{
    public int TotalFeeRecords { get; set; }

    public decimal TotalFeeAmount { get; set; }

    public decimal TotalPaidAmount { get; set; }

    public decimal TotalOutstandingAmount { get; set; }

    public int PendingFeeRecords { get; set; }

    public int PartiallyPaidFeeRecords { get; set; }

    public int PaidFeeRecords { get; set; }

    public int OverdueFeeRecords { get; set; }
}