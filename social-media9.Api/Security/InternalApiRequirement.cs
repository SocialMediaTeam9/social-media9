using Microsoft.AspNetCore.Authorization;

public class InternalApiRequirement : IAuthorizationRequirement
{
        public string RequiredSecret { get; }

    public InternalApiRequirement(string requiredSecret)
    {
        if (string.IsNullOrEmpty(requiredSecret))
        {
            throw new ArgumentNullException(nameof(requiredSecret), "A required secret must be provided for the InternalApiRequirement.");
        }
        RequiredSecret = requiredSecret;
    }
}