// =============================================================================
// File           : ResultFailureFactory.cs
// Layer          : Application / Common/Behaviors
// Purpose        : Builds a failed Result / Result<T> of a given response type from an Error.
// Logic          : Pipeline behaviors are generic over the response type but must short-circuit with a
//                  failure (validation, authorization); this helper creates the right closed type without the
//                  behavior knowing T.
// Implementation : Internal generic class; the factory delegate is built once per closed type through
//                  reflection and cached in a static field.
// =============================================================================
using System.Reflection;
using CardioSense.Domain.Common;

namespace CardioSense.Application.Common.Behaviors;

/// <summary>Creates failed results of an arbitrary Result response type.</summary>
internal static class ResultFailureFactory<TResponse>
{
    private static readonly Func<Error, TResponse> Factory = BuildFactory();

    /// <summary>Creates a failed result of type <typeparamref name="TResponse"/>.</summary>
    public static TResponse Create(Error error) => Factory(error);

    /// <summary>Resolves, once per closed type, how to create a failure for Result or Result&lt;T&gt;.</summary>
    private static Func<Error, TResponse> BuildFactory()
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return error => (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Result<T> declares its own Failure(Error) that returns Result<T>.
            var failure = responseType.GetMethod(
                nameof(Result.Failure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                binder: null,
                types: [typeof(Error)],
                modifiers: null)
                ?? throw new InvalidOperationException($"{responseType.Name} has no Failure(Error) factory.");

            return error => (TResponse)failure.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"Pipeline behaviors support only Result and Result<T> responses, not {responseType.Name}.");
    }
}
