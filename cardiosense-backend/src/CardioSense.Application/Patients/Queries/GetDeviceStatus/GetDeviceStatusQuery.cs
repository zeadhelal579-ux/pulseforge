// =============================================================================
// File           : GetDeviceStatusQuery.cs
// Layer          : Application / Patients/Queries/GetDeviceStatus
// Purpose        : Query for a device's status card data by serial number.
// Logic          : Serves GET /devices/{serial}/status. Not patient-scoped by id (the route has a serial), so
//                  the handler checks access itself through IPatientAccessChecker.
// Implementation : Immutable record implementing the Mediator request contract.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Queries.GetDeviceStatus;

/// <summary>Get a device's status.</summary>
public sealed record GetDeviceStatusQuery(string SerialNumber) : IRequest<Result<DeviceSummaryDto>>;
