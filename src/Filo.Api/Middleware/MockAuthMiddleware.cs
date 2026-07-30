using System.Security.Claims;
using Filo.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Filo.Api.Middleware;

public class MockAuthMiddleware
{
    private readonly RequestDelegate _next;

    public MockAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Okunan Header
        var userIdHeader = context.Request.Headers["x-user-id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(userIdHeader) && int.TryParse(userIdHeader, out int userId))
        {
            // Veritabanından kullanıcıyı bul
            // Middleware singleton/scoped farkı yüzünden IUnitOfWork'u scope içinden alıyoruz
            using var scope = context.RequestServices.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var person = await unitOfWork.Person.GetByIdAsync(userId);
            

            if (person != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, person.Id.ToString() ?? ""),
                    new Claim(ClaimTypes.Name, $"{person.Name} {person.Surname}"),
                    new Claim(ClaimTypes.Role, person.Role ?? "Staff")
                };

                var identity = new ClaimsIdentity(claims, "Mock");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}
