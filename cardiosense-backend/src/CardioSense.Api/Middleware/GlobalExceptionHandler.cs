// =============================================================================
// File           : GlobalExceptionHandler.cs
// Layer          : Api / Middleware
// Purpose        : Last-resort translation of an unexpected exception into a clean ProblemDetails response.
// Logic          : Expected failures are Results and never reach here. What does reach here is either a race the
//                  database caught (a unique index rejecting a duplicate that two requests both passed the
//                  application check for) - answered with 409 like the application-level check would have - or a real
//                  bug, answered with a generic 500 that leaks nothing (details go to the log, not to the client).
//                  A client that disconnected mid-request is not an error.
// Implementation : IExceptionHandler registered with AddExceptionHandler and triggered by UseExceptionHandler.
// =============================================================================
using CardioSense.Api.Http;
using CardioSense.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;

namespace CardioSense.Api.Middleware;

/// <summary>Maps unhandled exceptions to ProblemDetails responses.</summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>Writes the response for the exception; always reports it as handled.</summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        IResult response;
        if (UniqueViolation.IsCausedBy(exception))
        {
            logger.LogWarning(exception, "A unique constraint rejected {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);
            response = ApiErrors.UniqueViolation.ToProblem();
        }
        else
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);
            response = Results.Problem(
                detail: "An unexpected error occurred.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                extensions: new Dictionary<string, object?> { ["code"] = "Server.Unexpected" });
        }

        await response.ExecuteAsync(httpContext);
        return true;
    }
}
