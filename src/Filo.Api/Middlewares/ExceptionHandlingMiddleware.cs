using System.Net;
using System.Text.Json;
using Filo.Application.Exceptions;
using Filo.Common.Models;

namespace Filo.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Uygulama calisirken beklenmeyen bir hata olustu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException notFoundEx => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = ApiResponse<object>.FailureResponse(notFoundEx.Message, "Kaynak bulunamadi.")
            },
            ValidationException validationEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = ApiResponse<object>.FailureResponse(validationEx.Errors, "Dogrulama hatasi olustu.")
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = ApiResponse<object>.FailureResponse("Sistemde beklenmeyen bir hata olustu. Lütfen yöneticinizle iletisime gecin.", "Sunucu hatasi.")
            }
        };

        context.Response.StatusCode = response.StatusCode;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response.Body, jsonOptions));
    }

}
