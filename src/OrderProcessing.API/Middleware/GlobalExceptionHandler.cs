using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Domain.Exceptions;

namespace OrderProcessing.API.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, code) = exception switch
        {
            OrderNotFoundException => (HttpStatusCode.NotFound, "Order Not Found", ((DomainException)exception).Code),
            InvalidOrderOperationException => (HttpStatusCode.UnprocessableEntity, "Invalid Order Operation", ((DomainException)exception).Code),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", "INTERNAL_ERROR")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Domain exception: {Code}", code);

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Extensions = { ["code"] = code }
        };

        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
