// =============================================================================
// File           : GetPatientAiReportValidator.cs
// Layer          : Application / Patients/Queries/GetPatientAiReport
// Purpose        : Validates the period of the AI report query.
// Logic          : Only daily, weekly and monthly are accepted (the same set the frontend sends).
// Implementation : FluentValidation validator using the shared contract literals.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using FluentValidation;

namespace CardioSense.Application.Patients.Queries.GetPatientAiReport;

/// <summary>Rules for <see cref="GetPatientAiReportQuery"/>.</summary>
internal sealed class GetPatientAiReportValidator : AbstractValidator<GetPatientAiReportQuery>
{
    /// <summary>Declares the rules.</summary>
    public GetPatientAiReportValidator()
    {
        RuleFor(x => x.Period)
            .Must(period => ContractLiterals.ReportPeriods.TryResolve(period, out _))
            .WithMessage("Period must be 'daily', 'weekly' or 'monthly'.");
    }
}
