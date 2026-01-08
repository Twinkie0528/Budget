namespace Budget.Core.Interfaces;

/// <summary>
/// Provides access to the current authenticated user context.
/// </summary>
public interface ICurrentUser
{
    string UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

