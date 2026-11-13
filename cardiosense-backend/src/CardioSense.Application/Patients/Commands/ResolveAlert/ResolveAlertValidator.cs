// =============================================================================
// File           : ResolveAlertValidator.cs
// Layer          : Application / Patients/Commands/ResolveAlert
// Purpose        : Validates the resolve-alert request.
// Logic          : The alert id is required and the optional note respects the same length limit as the alert
//                  message.
// Implementation : FluentValidation validator using the entity constant.
// =============================================================================
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Patients.Commands.ResolveAlert;

/// <summary>Rules for <see cref="ResolveAlertCommand"/>.</summary>
internal sealed class ResolveAlertValidator : AbstractValidator<ResolveAlertCommand>
{
    /// <summary>Declares the rules.</summary>
    public ResolveAlertValidator()
    {
        RuleFor(x => x.AlertId).NotEmpty();
        RuleFor(x => x.ResolutionNote).MaximumLength(Alert.MessageMaxLength);
    }
}
