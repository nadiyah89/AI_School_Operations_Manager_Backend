namespace SchoolOperations.AI;

public class AIToolContext
{
    // JWT belonging to the authenticated user.
    public string AccessToken { get; }

    public AIToolContext(string accessToken)
    {
        AccessToken = accessToken;
    }
}