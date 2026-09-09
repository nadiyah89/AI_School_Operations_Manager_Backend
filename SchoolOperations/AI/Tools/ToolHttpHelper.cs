using System.Net.Http.Headers;

namespace SchoolOperations.AI.Tools;

public static class ToolHttpHelper
{
    public static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string requestUri,
        string accessToken,
        HttpContent? content = null)
    {
        var request = new HttpRequestMessage(
            method,
            requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        if (content != null)
        {
            request.Content = content;
        }

        return request;
    }
}