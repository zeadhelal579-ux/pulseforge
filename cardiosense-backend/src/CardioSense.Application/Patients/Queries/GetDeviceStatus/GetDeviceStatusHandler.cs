// =============================================================================
// File           : GetDeviceStatusHandler.cs
// Layer          : Application / Patients/Queries/GetDeviceStatus
// Purpose        : Handles GetDeviceStatusQuery.
// Logic          : A device the caller may not see answers 'not found' exactly like a missing one, so serial
//                  numbers cannot be enumerated to learn which devices are paired.
// Implementation : Internal sealed handler; ownership is decided by IPatientAccessChecker (patient = own
//                  device, doctor = assigned patient).
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Queries.GetDeviceStatus;

/// <summary>Handles <see cref="GetDeviceStatusQuery"/>.</summary>
internal sealed class GetDeviceStatusHandler(IAppDbContext db, IPatientAccessChecker access)
    : IRequestHandler<GetDeviceStatusQuery, Result<DeviceSummaryDto>>
{
    /// <summary>Returns the device summary when the caller may see it.</summary>
    public async ValueTask<Result<DeviceSummaryDto>> Handle(GetDeviceStatusQuery request, CancellationToken ct)
    {
        var serial = request.SerialNumber.Trim();
        var device = await db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.SerialNumber == serial, ct);

        if (device?.PatientId is not { } patientId || !await access.CanAccessAsync(patientId, ct))
        {
            return Result.Failure<DeviceSummaryDto>(ApplicationErrors.Device.NotFound);
        }

        return Result.Success(device.ToDto());
    }
}
