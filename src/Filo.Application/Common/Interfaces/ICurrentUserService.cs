namespace Filo.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Role { get; }
    bool IsAdmin { get; }
    bool IsManager { get; }
    bool IsStaff { get; }
}
