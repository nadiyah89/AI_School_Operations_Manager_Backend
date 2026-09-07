using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Google.GenAI.Types;

namespace SchoolOperations.AI.Tools;

public class CreateNotificationTool : ITool
{
    private readonly HttpClient _httpClient;


public CreateNotificationTool(
    HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // =========================================================
    // Tool Name
    // =========================================================

    public string Name =>
        "CreateNotification";


    // =========================================================
    // Tool Description
    // =========================================================

    public string Description =>
        "Creates and records a notification for a student and their parent. " +
        "The notification is stored in the school system with Pending status. " +
        "It does not physically send an SMS or Email. " +
        "Use this only when the user clearly asks to create or record a notification.";


    // =========================================================
    // Tool Parameters
    // =========================================================

    public Dictionary<string, Schema> Parameters =>
        new()
        {
            ["studentId"] = new Schema
            {
                Type = "integer",
                Description =
                    "The ID of the student associated with the notification."
            },

            ["parentId"] = new Schema
            {
                Type = "integer",
                Description =
                    "The ID of the parent associated with the notification."
            },

            ["notificationType"] = new Schema
            {
                Type = "string",
                Description =
                    "The type or category of the notification, such as Fee Reminder or Attendance Alert."
            },

            ["message"] = new Schema
            {
                Type = "string",
                Description =
                    "The notification message to record."
            },

            ["channel"] = new Schema
            {
                Type = "string",
                Description =
                    "The notification channel. Must be exactly SMS or Email."
            }
        };


    // =========================================================
    // Execute Tool
    // =========================================================

    public async Task<ToolResult> ExecuteAsync(
        Dictionary<string, object> arguments,
        AIToolContext context)
    {
        // ---------------------------------------------------------
        // 1. Validate student ID
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "studentId",
                out var studentIdValue) ||
            studentIdValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A student ID is required.");
        }

        int studentId;

        try
        {
            if (studentIdValue is JsonElement studentElement)
            {
                studentId =
                    studentElement.GetInt32();
            }
            else
            {
                studentId =
                    Convert.ToInt32(studentIdValue);
            }
        }
        catch
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The student ID must be a valid integer.");
        }

        if (studentId <= 0)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The student ID must be greater than zero.");
        }


        // ---------------------------------------------------------
        // 2. Validate parent ID
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "parentId",
                out var parentIdValue) ||
            parentIdValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A parent ID is required.");
        }

        int parentId;

        try
        {
            if (parentIdValue is JsonElement parentElement)
            {
                parentId =
                    parentElement.GetInt32();
            }
            else
            {
                parentId =
                    Convert.ToInt32(parentIdValue);
            }
        }
        catch
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The parent ID must be a valid integer.");
        }

        if (parentId <= 0)
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The parent ID must be greater than zero.");
        }


        // ---------------------------------------------------------
        // 3. Validate notification type
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "notificationType",
                out var notificationTypeValue) ||
            notificationTypeValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A notification type is required.");
        }

        var notificationType =
            GetStringValue(notificationTypeValue);

        if (string.IsNullOrWhiteSpace(notificationType))
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The notification type cannot be empty.");
        }

        notificationType =
            notificationType.Trim();


        // ---------------------------------------------------------
        // 4. Validate notification message
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "message",
                out var messageValue) ||
            messageValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A notification message is required.");
        }

        var message =
            GetStringValue(messageValue);

        if (string.IsNullOrWhiteSpace(message))
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The notification message cannot be empty.");
        }

        message =
            message.Trim();


        // ---------------------------------------------------------
        // 5. Validate channel
        // ---------------------------------------------------------

        if (!arguments.TryGetValue(
                "channel",
                out var channelValue) ||
            channelValue == null)
        {
            return ToolResult.Fail(
                "MissingArgument",
                "A notification channel is required.");
        }

        var channel =
            GetStringValue(channelValue);

        if (string.IsNullOrWhiteSpace(channel))
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The notification channel cannot be empty.");
        }

        channel =
            channel.Trim();

        if (channel != "SMS" &&
            channel != "Email")
        {
            return ToolResult.Fail(
                "InvalidArgument",
                "The notification channel must be SMS or Email.");
        }


        // ---------------------------------------------------------
        // 6. Validate JWT
        // ---------------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                context.AccessToken))
        {
            return ToolResult.Fail(
                "AuthenticationRequired",
                "An access token is required to create a notification.");
        }


        // ---------------------------------------------------------
        // 7. Attach JWT
        // ---------------------------------------------------------

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                context.AccessToken);


        // ---------------------------------------------------------
        // 8. Create request payload
        // ---------------------------------------------------------

        var payload =
            new CreateNotificationRequestDto
            {
                StudentId = studentId,
                ParentId = parentId,
                NotificationType = notificationType,
                Message = message,
                Channel = channel
            };


        // ---------------------------------------------------------
        // 9. Serialize request
        // ---------------------------------------------------------

        var json =
            JsonSerializer.Serialize(payload);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");


        // ---------------------------------------------------------
        // 10. Call backend API
        // ---------------------------------------------------------

        HttpResponseMessage response;

        try
        {
            response =
                await _httpClient.PostAsync(
                    "https://localhost:7003/api/notifications",
                    content);
        }
        catch (HttpRequestException)
        {
            return ToolResult.Fail(
                "BackendUnavailable",
                "The notifications backend could not be reached.");
        }


        // ---------------------------------------------------------
        // 11. Handle backend errors
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
                        "Authentication is required to create a notification.");


                case HttpStatusCode.Forbidden:

                    return ToolResult.Fail(
                        "Forbidden",
                        "The authenticated user does not have permission " +
                        "to create notifications.");


                case HttpStatusCode.NotFound:

                    return ToolResult.Fail(
                        "NotFound",
                        string.IsNullOrWhiteSpace(error)
                            ? "The requested student or parent does not exist."
                            : error);


                case HttpStatusCode.BadRequest:

                    return ToolResult.Fail(
                        "BadRequest",
                        string.IsNullOrWhiteSpace(error)
                            ? "The notification request was invalid."
                            : error);


                default:

                    return ToolResult.Fail(
                        $"HttpError_{(int)response.StatusCode}",
                        "The notifications backend returned an unexpected error.");
            }
        }


        // ---------------------------------------------------------
        // 12. Read successful response
        // ---------------------------------------------------------

        var responseJson =
            await response.Content.ReadAsStringAsync();


        // ---------------------------------------------------------
        // 13. Deserialize created notification
        // ---------------------------------------------------------

        // ---------------------------------------------------------
        // 13. Deserialize created notification
        // ---------------------------------------------------------

        CreatedNotificationResultDto? result;

        try
        {
            result =
                JsonSerializer.Deserialize<
                    CreatedNotificationResultDto>(
                    responseJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }
        catch (JsonException)
        {
            return ToolResult.Fail(
                "InvalidJson",
                "The backend returned an unexpected data format.");
        }

        // ---------------------------------------------------------
        // 14. Validate response
        // ---------------------------------------------------------

        if (result == null)
        {
            return ToolResult.Fail(
                "InvalidResponse",
                "The notifications backend returned an empty or invalid response.");
        }


        // ---------------------------------------------------------
        // 15. Return successful result
        // ---------------------------------------------------------

        return ToolResult.Ok(result);
    }


    // =========================================================
    // Convert Gemini argument to string safely
    // =========================================================

    private static string? GetStringValue(
        object value)
    {
        if (value is JsonElement element)
        {
            if (element.ValueKind !=
                JsonValueKind.String)
            {
                return null;
            }

            return element.GetString();
        }

        return value.ToString();
    }


}

// =============================================================
// Request DTO sent to the backend
// =============================================================

public class CreateNotificationRequestDto
{
    public int StudentId { get; set; }


public int ParentId { get; set; }

    public string NotificationType { get; set; }
        = string.Empty;

    public string Message { get; set; }
        = string.Empty;

    public string Channel { get; set; }
        = string.Empty;


}

// =============================================================
// Notification returned after successful creation
// =============================================================

public class CreatedNotificationResultDto
{
    public int Id { get; set; }


public int StudentId { get; set; }

    public int ParentId { get; set; }

    public string NotificationType { get; set; }
        = string.Empty;

    public string Message { get; set; }
        = string.Empty;

    public string Channel { get; set; }
        = string.Empty;

    public string Status { get; set; }
        = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public bool IsActive { get; set; }


}
