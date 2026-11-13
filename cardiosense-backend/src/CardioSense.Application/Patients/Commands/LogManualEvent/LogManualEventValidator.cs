// =============================================================================
// File           : LogManualEventValidator.cs
// Layer          : Application / Patients/Commands/LogManualEvent
// Purpose        : Validates the manual event request.
// Logic          : A note is required (it becomes the alert message) and the severity must be a defined
//                  AlertSeverity value.
// Implementation : FluentValidation validator using the entity constant for the length limit.
// =============================================================================
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Patients.Commands.LogManualEvent;

/// <summary>Rules for <see cref="LogManualEventCommand"/>.</summary>
internal sealed class LogManualEventValidator : AbstractValidator<LogManualEventCommand>
{
    /// <summary>Declares the rules.</summary>
    public LogManualEventValidator()
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(Alert.MessageMaxLength);
        RuleFor(x => x.Severity).IsInEnum();
    }
}
