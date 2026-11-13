// =============================================================================
// File           : GetMyPatientsValidator.cs
// Layer          : Application / Doctors/Queries/GetMyPatients
// Purpose        : Validates the patient list query parameters.
// Logic          : Status must be a known filter, the page is 1-based and the page size is capped.
// Implementation : FluentValidation validator using the shared contract literals.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using FluentValidation;

namespace CardioSense.Application.Doctors.Queries.GetMyPatients;

/// <summary>Rules for <see cref="GetMyPatientsQuery"/>.</summary>
internal sealed class GetMyPatientsValidator : AbstractValidator<GetMyPatientsQuery>
{
    /// <summary>Longest search text accepted.</summary>
    private const int SearchMaxLength = 100;

    /// <summary>Declares the rules.</summary>
    public GetMyPatientsValidator()
    {
        RuleFor(x => x.Status)
            .Must(ContractLiterals.PatientListFilters.IsSupported)
            .WithMessage("Status must be 'all', 'stable', 'alert' or 'critical'.");
        RuleFor(x => x.Search).MaximumLength(SearchMaxLength);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetMyPatientsQuery.MaxPageSize);
    }
}
