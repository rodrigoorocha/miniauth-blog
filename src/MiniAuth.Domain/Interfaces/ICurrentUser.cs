namespace MiniAuth.Domain.Interfaces;

public interface ICurrentUser
{
    string UserId { get; }
    string Email { get; }
    string FullName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
