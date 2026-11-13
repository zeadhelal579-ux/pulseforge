// =============================================================================
// File           : Result.cs
// Layer          : Domain / Common
// Purpose        : The Result pattern - every use case and domain rule returns success or an Error.
// Logic          : A Result is either successful (no error) or failed (exactly one Error); Result<T> adds a
//                  value that can only be read when the result succeeded.
// Implementation : Result and Result<T> are intentionally declared together because they form one concept.
//                  Constructors enforce the success/error invariant so an invalid Result cannot exist.
// =============================================================================
namespace CardioSense.Domain.Common;

/// <summary>Outcome of an operation without a return value.</summary>
public class Result
{
    /// <summary>Creates a result and enforces the success/error invariant.</summary>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failed result must carry an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>True when the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>True when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The failure description; <see cref="Error.None"/> on success.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result carrying a value.</summary>
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    /// <summary>Creates a failed result of a value-carrying type.</summary>
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

/// <summary>Outcome of an operation that returns a value on success.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>Creates the result; only the static factories on <see cref="Result"/> call this.</summary>
    internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>The success value; reading it from a failed result is a programming error.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    /// <summary>Creates a failed result of this type (hides the non-generic factory on purpose).</summary>
    public static new Result<T> Failure(Error error) => new(default, false, error);
}
