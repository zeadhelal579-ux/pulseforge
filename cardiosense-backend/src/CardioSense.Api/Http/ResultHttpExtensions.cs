// =============================================================================
// File           : ResultHttpExtensions.cs
// Layer          : Api / Http
// Purpose        : Turns a Result or an Error from the mediator into the HTTP response.
// Logic          : The one mapping from ErrorType to HTTP status (Failure 422, Validation 400, Unauthorized 401,
//                  Forbidden 403, NotFound 404, Conflict 409, Unavailable 503). Every failure is an RFC 7807
//                  ProblemDetails with title, status, detail and a stable machine-readable "code"; validation
//                  failures also carry the per-field "errors" dictionary (camelCase field names) the frontend binds
//                  to its form controls.
// Implementation : Extension methods used by every endpoint, so an endpoint body is one line.
// =============================================================================
using CardioSense.Domain.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace CardioSense.Api.Http;

/// <summary>HTTP translation of Result and Error.</summary>
internal static class ResultHttpExtensions
{
    /// <summary>200 with the value, or the problem response.</summary>
    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();

    /// <summary>201 with the value and the location of the created resource, or the problem response.</summary>
    public static IResult ToCreatedResult<T>(this Result<T> result, string location) =>
        result.IsSuccess ? TypedResults.Created(location, result.Value) : result.Error.ToProblem();

    /// <summary>204 when it worked, or the problem response.</summary>
    public static IResult ToNoContentResult(this Result result) =>
        result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();

    /// <summary>Builds the ProblemDetails response for an error.</summary>
    public static IResult ToProblem(this Error error)
    {
        var status = StatusOf(error.Type);
        var title = ReasonPhrases.GetReasonPhrase(status);
        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };

        if (error is { Type: ErrorType.Validation, ValidationErrors: { } fieldErrors })
        {
            return Results.ValidationProblem(
                fieldErrors.ToDictionary(field => field.Key, field => field.Value),
                detail: error.Description,
                statusCode: status,
                title: title,
                extensions: extensions);
        }

        return Results.Problem(detail: error.Description, statusCode: status, title: title, extensions: extensions);
    }

    /// <summary>The HTTP status for an error category.</summary>
    private static int StatusOf(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status422UnprocessableEntity
    };
}
