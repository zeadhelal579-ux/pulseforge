// =============================================================================
// File           : RegisterPatientHandler.cs
// Layer          : Application / Auth/Commands/RegisterPatient
// Purpose        : Handles patient signup: account + profile + medical history, optional doctor link and
//                  device pairing, then a session.
// Logic          : Account and profile are created in ONE transaction so no orphan login can exist. The
//                  doctor name and device serial are optional extras: they are linked only when they resolve
//                  unambiguously (exactly one doctor with that name, an inventory device that is free) -
//                  otherwise signup still succeeds and the user can pair later in Settings.
// Implementation : Internal sealed handler; the optional links are best-effort helpers that log (never the
//                  personal data itself) and never fail the signup.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CardioSense.Application.Auth.Commands.RegisterPatient;

/// <summary>Handles <see cref="RegisterPatientCommand"/>.</summary>
internal sealed class RegisterPatientHandler(
    IAppDbContext db,
    IIdentityService identity,
    SessionIssuer sessions,
    TimeProvider clock,
    ILogger<RegisterPatientHandler> logger)
    : IRequestHandler<RegisterPatientCommand, Result<AuthSession>>
{
    /// <summary>Creates the patient and returns a signed-in session.</summary>
    public async ValueTask<Result<AuthSession>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var nationalId = request.NationalId.Trim();
        if (await db.Patients.AnyAsync(p => p.NationalId == nationalId, ct))
        {
            return Result.Failure<AuthSession>(ApplicationErrors.Patient.NationalIdTaken);
        }

        var now = clock.GetUtcNow();
        var email = request.Email.Trim();

        // One transaction covers the login account and the profile rows.
        await using var transaction = await db.BeginTransactionAsync(ct);

        var created = await identity.CreateUserAsync(
            new NewUser(email, request.Password, UserRole.Patient, request.PreferredLanguage), ct);
        if (created.IsFailure)
        {
            return Result.Failure<AuthSession>(created.Error);
        }

        var patient = Patient.Create(
            created.Value,
            request.FullName,
            Patient.DateOfBirthFromAge(request.Age, DateOnly.FromDateTime(now.UtcDateTime)),
            ContractLiterals.Genders.Parse(request.Gender),
            nationalId,
            request.Phone,
            now);
        db.Patients.Add(patient);

        AddHistory(patient.Id, MedicalHistoryEntryType.Diagnosis, request.Diagnosis, now);
        AddHistory(patient.Id, MedicalHistoryEntryType.Medication, request.Medications, now);

        await LinkDoctorByNameAsync(patient.Id, request.AssignedDoctorName, now, ct);
        await PairDeviceAsync(patient.Id, request.DeviceSerial, now, ct);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return await sessions.IssueAsync(new UserAccount(created.Value, email, UserRole.Patient,
            request.PreferredLanguage), ct);
    }

    /// <summary>Adds a history row when the text is not blank.</summary>
    private void AddHistory(Guid patientId, MedicalHistoryEntryType type, string? description, DateTimeOffset now)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            db.MedicalHistoryEntries.Add(MedicalHistoryEntry.Create(patientId, type, description, now));
        }
    }

    /// <summary>
    /// Assigns the doctor only when exactly one doctor has that name (never guess between namesakes).
    /// </summary>
    private async Task LinkDoctorByNameAsync(Guid patientId, string? doctorName, DateTimeOffset now,
        CancellationToken ct)
    {
        var name = doctorName?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        var matches = await db.Doctors.AsNoTracking()
            .Where(d => d.FullName == name)
            .Select(d => d.Id)
            .Take(2)
            .ToListAsync(ct);

        if (matches.Count != 1)
        {
            logger.LogInformation("Signup doctor name matched {MatchCount} doctors; no assignment created.",
            matches.Count);
            return;
        }

        db.DoctorPatientAssignments.Add(DoctorPatientAssignment.Create(matches[0], patientId, true, now));
    }

    /// <summary>Pairs the device when it is an inventory device that is free or already this patient's.</summary>
    private async Task PairDeviceAsync(Guid patientId, string? deviceSerial, DateTimeOffset now, CancellationToken ct)
    {
        var serial = deviceSerial?.Trim();
        if (string.IsNullOrEmpty(serial))
        {
            return;
        }

        var device = await db.Devices.FirstOrDefaultAsync(d => d.SerialNumber == serial, ct);
        if (device is null)
        {
            logger.LogInformation("Signup device serial is not in the inventory; pairing skipped.");
            return;
        }

        var paired = device.PairTo(patientId, now);
        if (paired.IsFailure)
        {
            logger.LogInformation("Signup device pairing skipped: {ErrorCode}.", paired.Error.Code);
        }
    }
}
