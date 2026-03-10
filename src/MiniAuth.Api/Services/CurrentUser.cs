using System.Security.Claims;
using MiniAuth.Domain.Interfaces;

namespace MiniAuth.Api.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public string UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    public string Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    public string FullName => User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
