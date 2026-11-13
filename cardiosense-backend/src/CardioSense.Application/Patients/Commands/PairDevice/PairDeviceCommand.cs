// =============================================================================
// File           : PairDeviceCommand.cs
// Layer          : Application / Patients/Commands/PairDevice
// Purpose        : Request to pair a device (by serial number) to a patient.
// Logic          : Body of POST /api/devices/pair. A patient omits PatientId (it means 'me'); a doctor must
//                  send the patient's id.
// Implementation : Immutable record; not patient-scoped through the marker because PatientId is optional -
//                  the handler checks access itself.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Commands.PairDevice;

/// <summary>Pair a device to a patient.</summary>
public sealed record PairDeviceCommand(string SerialNumber, Guid? PatientId) : IRequest<Result<DeviceSummaryDto>>;
