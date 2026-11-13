// =============================================================================
// File           : DoctorEndpoints.cs
// Layer          : Api / Endpoints
// Purpose        : /api/doctors/me - the signed-in doctor's profile, caseload list, ward statistics and "Add New
//                  Patient".
// Logic          : Everything here is scoped to the caller (a doctor never passes a doctor id), and the whole group is
//                  doctor-only. The patient list is "patients assigned to this doctor", enforced in the Application
//                  query, not by a UI filter. Omitted list parameters fall back to the defaults the frontend expects.
// Implementation : One mediator call per endpoint.
// =============================================================================
using CardioSense.Api.Authorization;
using CardioSense.Api.Http;
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Doctors.Commands.UpdateDoctorProfile;
using CardioSense.Application.Doctors.Queries.GetDoctorProfile;
using CardioSense.Application.Doctors.Queries.GetMyPatients;
using CardioSense.Application.Doctors.Queries.GetWardStats;
using CardioSense.Application.Patients.Commands.AssignDoctor;
using Mediator;

namespace CardioSense.Api.Endpoints;

/// <summary>Doctor endpoints.</summary>
internal sealed class DoctorEndpoints : IEndpointGroup
{
    /// <summary>Maps the /api/doctors/me routes.</summary>
    public void Map(IEndpointRouteBuilder app)
    {
        var doctor = app.MapGroup("/api/doctors/me")
            .WithTags("Doctors")
            .RequireAuthorization(AuthorizationPolicies.DoctorOnly);

        doctor.MapGet("/", GetProfile);
        doctor.MapPut("/", UpdateProfile);
        doctor.MapGet("/patients", GetPatients);
        doctor.MapPost("/patients", AssignPatient);
        doctor.MapGet("/ward-stats", GetWardStats);
    }

    /// <summary>GET /api/doctors/me.</summary>
    private static async Task<IResult> GetProfile(ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetDoctorProfileQuery(), ct)).ToHttpResult();

    /// <summary>PUT /api/doctors/me - partial update (the license number is never editable).</summary>
    private static async Task<IResult> UpdateProfile(
        UpdateDoctorProfileCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    /// <summary>GET /api/doctors/me/patients?status=&amp;search=&amp;page=&amp;pageSize=.</summary>
    private static async Task<IResult> GetPatients(
        string? status, string? search, int? page, int? pageSize, ISender sender, CancellationToken ct)
    {
        var query = new GetMyPatientsQuery(
            status ?? ContractLiterals.PatientListFilters.All,
            search,
            page ?? 1,
            pageSize ?? GetMyPatientsQuery.DefaultPageSize);

        return (await sender.Send(query, ct)).ToHttpResult();
    }

    /// <summary>POST /api/doctors/me/patients - "Add New Patient" by national id.</summary>
    private static async Task<IResult> AssignPatient(
        AssignDoctorCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    /// <summary>GET /api/doctors/me/ward-stats.</summary>
    private static async Task<IResult> GetWardStats(ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetWardStatsQuery(), ct)).ToHttpResult();
}
