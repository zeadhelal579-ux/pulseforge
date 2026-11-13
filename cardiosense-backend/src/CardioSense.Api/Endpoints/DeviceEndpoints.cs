// =============================================================================
// File           : DeviceEndpoints.cs
// Layer          : Api / Endpoints
// Purpose        : /api/devices - pair a device to a patient and read a device's status card.
// Logic          : Open to any signed-in user because both roles use it (a patient pairs their own device, a doctor
//                  pairs one for a patient). Who may do what is decided by the handlers through
//                  IPatientAccessChecker, and a device the caller has no right to is reported as not found so serial
//                  numbers cannot be guessed.
// Implementation : One mediator call per endpoint.
// =============================================================================
using CardioSense.Api.Http;
using CardioSense.Application.Patients.Commands.PairDevice;
using CardioSense.Application.Patients.Queries.GetDeviceStatus;
using Mediator;

namespace CardioSense.Api.Endpoints;

/// <summary>Device endpoints.</summary>
internal sealed class DeviceEndpoints : IEndpointGroup
{
    /// <summary>Maps the /api/devices routes.</summary>
    public void Map(IEndpointRouteBuilder app)
    {
        var devices = app.MapGroup("/api/devices").WithTags("Devices").RequireAuthorization();

        devices.MapPost("/pair", PairDevice);
        devices.MapGet("/{serial}/status", GetStatus);
    }

    /// <summary>POST /api/devices/pair.</summary>
    private static async Task<IResult> PairDevice(
        PairDeviceCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    /// <summary>GET /api/devices/{serial}/status.</summary>
    private static async Task<IResult> GetStatus(string serial, ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetDeviceStatusQuery(serial), ct)).ToHttpResult();
}
