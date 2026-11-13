// =============================================================================
// File           : GetPatientVitalsHistoryValidator.cs
// Layer          : Application / Patients/Queries/GetPatientVitalsHistory
// Purpose        : Validates the range of the vitals history query.
// Logic          : Only the literals 24h, 7d and 30d are accepted (the same set the frontend sends).
// Implementation : FluentValidation validator using the shared contract literals.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using FluentValidation;

namespace CardioSense.Application.Patients.Queries.GetPatientVitalsHistory;

/// <summary>Rules for <see cref="GetPatientVitalsHistoryQuery"/>.</summary>
internal sealed class GetPatientVitalsHistoryValidator : AbstractValidator<GetPatientVitalsHistoryQuery>
{
    /// <summary>Declares the rules.</summary>
    public GetPatientVitalsHistoryValidator()
    {
        RuleFor(x => x.Range)
            .Must(range => ContractLiterals.VitalsRanges.TryResolve(range, out _, out _))
            .WithMessage("Range must be '24h', '7d' or '30d'.");
    }
}
