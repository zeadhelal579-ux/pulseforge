// =============================================================================
// File           : DomainException.cs
// Layer          : Domain / Exceptions
// Purpose        : Base exception for domain invariants that were broken by a programming error.
// Logic          : Expected failures use Result; this exception is reserved for states that should never
//                  occur once input validation has run (for example an empty Id passed to a factory).
// Implementation : Minimal Exception subclass with a message constructor.
// =============================================================================
namespace CardioSense.Domain.Exceptions;

/// <summary>Raised when a domain invariant is violated by the caller.</summary>
public class DomainException : Exception
{
    /// <summary>Creates the exception with a human-readable message.</summary>
    public DomainException(string message)
        : base(message)
    {
    }
}
