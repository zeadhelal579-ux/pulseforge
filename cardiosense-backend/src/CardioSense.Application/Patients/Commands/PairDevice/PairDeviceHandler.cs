// =============================================================================
// File           : PairDeviceHandler.cs
// Layer          : Application / Patients/Commands/PairDevice
// Purpose        : Handles device pairing: resolves the target patient, checks access, then pairs an
//                  inventory device.
// Logic          : Only devices that already exist in the inventory can be paired (unknown serials are 'not
//                  found'); one patient can hold one device, and a device cannot belong to two patients.
//                  Replacing a device silently could stop monitoring, so a second pairing is refused instead.
// Implementation : Internal sealed handler; the 'already paired to another patient' rule lives in
//                  Device.PairTo.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Commands.PairDevice;

/// <summary>Handles <see cref="PairDeviceCommand"/>.</summary>
internal sealed class PairDeviceHandler(
    IAppDbContext db, IPatientAccessChecker access, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<PairDeviceCommand, Result<DeviceSummaryDto>>
{
    /// <summary>Pairs the device to the resolved patient.</summary>
    public async ValueTask<Result<DeviceSummaryDto>> Handle(PairDeviceCommand request, CancellationToken ct)
    {
        // A patient pairs for themself; a doctor has to say which patient.
        if ((request.PatientId ?? currentUser.PatientId) is not { } patientId)
        {
            return Result.Failure<DeviceSummaryDto>(ApplicationErrors.Device.PatientRequired);
        }

        if (!await access.CanAccessAsync(patientId, ct))
        {
            return Result.Failure<DeviceSummaryDto>(ApplicationErrors.Patient.AccessDenied);
        }

        var serial = request.SerialNumber.Trim();
        var device = await db.Devices.FirstOrDefaultAsync(d => d.SerialNumber == serial, ct);
        if (device is null)
        {
            return Result.Failure<DeviceSummaryDto>(ApplicationErrors.Device.NotFound);
        }

        var alreadyHasAnotherDevice = await db.Devices.AnyAsync(d => d.PatientId == patientId && d.Id != device.Id, ct);
        if (alreadyHasAnotherDevice)
        {
            return Result.Failure<DeviceSummaryDto>(ApplicationErrors.Patient.AlreadyHasDevice);
        }

        var paired = device.PairTo(patientId, clock.GetUtcNow());
        if (paired.IsFailure)
        {
            return Result.Failure<DeviceSummaryDto>(paired.Error);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success(device.ToDto());
    }
}
