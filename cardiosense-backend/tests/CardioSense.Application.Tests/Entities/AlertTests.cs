// =============================================================================
// File           : AlertTests.cs
// Layer          : Tests / Entities
// Purpose        : Verifies the alert rules that protect patients: a critical alert is released only by a doctor.
// Logic          : The rule lives on the entity, so it holds no matter which use case touches the alert, now or in a
//                  future feature.
// Implementation : Plain xUnit facts; no database.
// =============================================================================
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Tests.Entities;

/// <summary>Tests for the Alert entity.</summary>
public sealed class AlertTests
{
    /// <summary>The instant used as "now".</summary>
    private static readonly DateTimeOffset Now = ApplicationTestHost.Start;

    /// <summary>Builds an unresolved alert of the given severity.</summary>
    private static Alert Open(AlertSeverity severity) =>
        Alert.Create(Guid.NewGuid(), AlertType.RiskThreshold, severity, "Test alert.", Now);

    /// <summary>The system (not a doctor) must not be able to close a critical alert.</summary>
    [Fact]
    public void A_critical_alert_cannot_be_resolved_without_a_doctor()
    {
        var alert = Open(AlertSeverity.Critical);

        var result = alert.Resolve("auto", resolvedByDoctor: false, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(DomainErrors.Alert.CriticalRequiresDoctor.Code, result.Error.Code);
        Assert.Null(alert.ResolvedAt);
    }

    /// <summary>A doctor can close a critical alert and the resolution is recorded.</summary>
    [Fact]
    public void A_doctor_can_resolve_a_critical_alert()
    {
        var alert = Open(AlertSeverity.Critical);

        var result = alert.Resolve("Reviewed, patient stable.", resolvedByDoctor: true, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, alert.ResolvedAt);
        Assert.Equal("Reviewed, patient stable.", alert.ResolutionNote);
    }

    /// <summary>Lower-severity alerts may be closed automatically when the status recovers.</summary>
    [Fact]
    public void A_moderate_alert_can_be_resolved_automatically()
    {
        var alert = Open(AlertSeverity.Moderate);

        var result = alert.Resolve(null, resolvedByDoctor: false, Now);

        Assert.True(result.IsSuccess);
        Assert.NotNull(alert.ResolvedAt);
    }

    /// <summary>Resolving twice is a conflict, so the first resolution (and its note) is never overwritten.</summary>
    [Fact]
    public void An_alert_cannot_be_resolved_twice()
    {
        var alert = Open(AlertSeverity.Moderate);
        alert.Resolve("first", resolvedByDoctor: true, Now);

        var result = alert.Resolve("second", resolvedByDoctor: true, Now.AddMinutes(1));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("first", alert.ResolutionNote);
    }
}
