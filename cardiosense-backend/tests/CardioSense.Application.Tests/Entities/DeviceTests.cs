// =============================================================================
// File           : DeviceTests.cs
// Layer          : Tests / Entities
// Purpose        : Verifies device pairing and "last seen" rules.
// Logic          : A device belongs to one patient at a time (silently re-pairing could stop someone's monitoring),
//                  and "last seen" only ever moves forward so a late, out-of-order message cannot make a live device
//                  look dead.
// Implementation : Plain xUnit facts; no database.
// =============================================================================
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;

namespace CardioSense.Application.Tests.Entities;

/// <summary>Tests for the Device entity.</summary>
public sealed class DeviceTests
{
    /// <summary>The instant used as "now".</summary>
    private static readonly DateTimeOffset Now = ApplicationTestHost.Start;

    /// <summary>A device already paired to one patient cannot be paired to another.</summary>
    [Fact]
    public void A_device_cannot_be_paired_to_a_second_patient()
    {
        var device = Device.Register("CS-0001", Now);
        device.PairTo(Guid.NewGuid(), Now);

        var result = device.PairTo(Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(DomainErrors.Device.AlreadyPaired.Code, result.Error.Code);
    }

    /// <summary>Pairing the same patient again is harmless and keeps the first pairing time.</summary>
    [Fact]
    public void Pairing_the_same_patient_again_is_idempotent()
    {
        var patientId = Guid.NewGuid();
        var device = Device.Register("CS-0001", Now);
        device.PairTo(patientId, Now);

        var result = device.PairTo(patientId, Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, device.PairedAt);
    }

    /// <summary>An older "seen" time never replaces a newer one.</summary>
    [Fact]
    public void Last_seen_never_moves_backwards()
    {
        var device = Device.Register("CS-0001", Now);
        device.RecordSeen(Now.AddMinutes(5));

        device.RecordSeen(Now.AddMinutes(1));

        Assert.Equal(Now.AddMinutes(5), device.LastSeenAt);
    }

    /// <summary>A device is live only while its last message is inside the window.</summary>
    [Fact]
    public void A_device_is_live_only_inside_the_window()
    {
        var device = Device.Register("CS-0001", Now);
        device.RecordSeen(Now);

        Assert.True(device.IsLive(Now.AddSeconds(60), TimeSpan.FromSeconds(120)));
        Assert.False(device.IsLive(Now.AddSeconds(121), TimeSpan.FromSeconds(120)));
    }
}
