// =============================================================================
// File           : VitalsSampleTests.cs
// Layer          : Tests / Entities
// Purpose        : Verifies the sensor sanity limits on a vitals reading.
// Logic          : The limits are deliberately wide (heart rate 20-300, SpO2 50-100) so genuine pathological values
//                  pass, while a sensor artifact (0, 999) is rejected instead of being charted as a real reading.
// Implementation : Theories on both sides of each boundary; no database.
// =============================================================================
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;

namespace CardioSense.Application.Tests.Entities;

/// <summary>Tests for the VitalsSample entity.</summary>
public sealed class VitalsSampleTests
{
    /// <summary>Creates a sample with the given values.</summary>
    private static Result<VitalsSample> Create(int heartRate, int spo2) =>
        VitalsSample.Create(Guid.NewGuid(), Guid.NewGuid(), ApplicationTestHost.Start, heartRate, spo2);

    /// <summary>Boundary heart rates are accepted.</summary>
    [Theory]
    [InlineData(20)]
    [InlineData(74)]
    [InlineData(300)]
    public void Plausible_heart_rates_are_accepted(int heartRate) => Assert.True(Create(heartRate, 98).IsSuccess);

    /// <summary>Values just outside the range are rejected.</summary>
    [Theory]
    [InlineData(19)]
    [InlineData(301)]
    [InlineData(0)]
    public void Impossible_heart_rates_are_rejected(int heartRate) => Assert.True(Create(heartRate, 98).IsFailure);

    /// <summary>Boundary oxygen saturations are accepted.</summary>
    [Theory]
    [InlineData(50)]
    [InlineData(97)]
    [InlineData(100)]
    public void Plausible_oxygen_saturations_are_accepted(int spo2) => Assert.True(Create(74, spo2).IsSuccess);

    /// <summary>Values just outside the range are rejected.</summary>
    [Theory]
    [InlineData(49)]
    [InlineData(101)]
    public void Impossible_oxygen_saturations_are_rejected(int spo2) => Assert.True(Create(74, spo2).IsFailure);
}
