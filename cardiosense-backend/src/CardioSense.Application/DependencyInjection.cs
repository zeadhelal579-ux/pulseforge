// =============================================================================
// File           : DependencyInjection.cs
// Layer          : Application / (root)
// Purpose        : Single entry point that registers everything the Application layer needs (Mediator,
//                  validators, services, options).
// Logic          : Called once from the host: services.AddApplication(). The Mediator source generator lives
//                  in this assembly so it can discover the internal handlers; pipeline order is Authorization
//                  first, then Validation, then the handler.
// Implementation : Static extension method; also registers TimeProvider.System when the host has not
//                  registered a clock yet.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Application.Common.Behaviors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Options;
using CardioSense.Application.Common.Security;
using CardioSense.Application.Doctors.Common;
using CardioSense.Application.Patients.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CardioSense.Application;

/// <summary>Application layer registration.</summary>
public static class DependencyInjection
{
    /// <summary>Registers handlers, pipeline behaviors, validators, shared services and default options.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AddMediator is produced by the Mediator source generator (same assembly).
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(AuthorizationBehavior<,>), typeof(ValidationBehavior<,>)];
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        // Defaults apply even if the host never binds the "Monitoring" configuration section.
        services.AddOptions<MonitoringOptions>();
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IPatientAccessChecker, PatientAccessChecker>();
        services.AddScoped<SessionIssuer>();
        services.AddScoped<PatientProfileAssembler>();
        services.AddScoped<PatientListItemAssembler>();

        return services;
    }
}
