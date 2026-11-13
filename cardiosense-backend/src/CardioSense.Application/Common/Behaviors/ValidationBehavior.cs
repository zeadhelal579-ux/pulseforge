// =============================================================================
// File           : ValidationBehavior.cs
// Layer          : Application / Common/Behaviors
// Purpose        : Mediator pipeline behavior that runs every FluentValidation validator before the handler.
// Logic          : Invalid input never reaches a handler: the pipeline returns a failed Result of type
//                  Validation carrying field-level messages (camelCase field names) which the Api turns into
//                  a 400 ProblemDetails.
// Implementation : Open generic behavior registered in AddApplication; validators run one after another so a
//                  validator may safely use a scoped service later. A message without validators passes
//                  straight through.
// =============================================================================
using CardioSense.Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace CardioSense.Application.Common.Behaviors;

/// <summary>Validates messages before they are handled.</summary>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    /// <summary>Runs the validators; continues to the handler only when all pass.</summary>
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TMessage>(message);
        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        // Group messages by field so the client can show them next to the right input.
        var fieldErrors = failures
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        var error = Error.Validation("Validation.Failed", "One or more validation errors occurred.", fieldErrors);
        return ResultFailureFactory<TResponse>.Create(error);
    }

    /// <summary>Converts "Address.PostalCode" to "address.postalCode" (each segment starts lower-case).</summary>
    private static string ToCamelCase(string propertyName) =>
        string.Join(
            '.',
            propertyName.Split('.').Select(segment =>
                segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
}
