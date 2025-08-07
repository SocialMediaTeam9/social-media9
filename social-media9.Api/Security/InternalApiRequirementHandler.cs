using Microsoft.AspNetCore.Authorization;

public class InternalApiRequirementHandler : AuthorizationHandler<InternalApiRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InternalApiRequirementHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InternalApiRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Gotosocial-Hook-Secret", out var receivedSecretHeaderValue))
        {
            if (receivedSecretHeaderValue.ToString() == requirement.RequiredSecret)
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}