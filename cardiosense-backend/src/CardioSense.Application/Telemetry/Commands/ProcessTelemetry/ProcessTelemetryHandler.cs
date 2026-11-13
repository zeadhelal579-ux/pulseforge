// =============================================================================
// File           : ProcessTelemetryHandler.cs
// Layer          : Application / Telemetry/Commands/ProcessTelemetry
// Purpose        : The core use case: device message in -> stored vitals/features -> live push -> AI
//                  assessment -> alerts -> push of the result.
// Logic          : Order matters: what the device measured is saved and shown FIRST, so an AI outage never
//                  loses data (it only means no new assessment). Alert policy: a new Critical alert only when
//                  none is open, a Moderate alert when Stable turns to Caution, and open non-critical risk
//                  alerts auto-resolve when the status returns to Stable. The critical latch lives in
//                  RiskAssessment.Create; this handler only tells it whether a critical alert is open.
// Implementation : Internal sealed handler split into small private steps. Live pushes are best-effort; the
//                  doctor list gets a light summary.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using CardioSense.Domain.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CardioSense.Application.Telemetry.Commands.ProcessTelemetry;

/// <summary>Handles <see cref="ProcessTelemetryCommand"/>.</summary>
internal sealed class ProcessTelemetryHandler(
    IAppDbContext db,
    IVitalsTimeSeriesStore vitals,
    IRiskAssessmentService ai,
    IRealtimeNotifier notifier,
    TimeProvider clock,
    ILogger<ProcessTelemetryHandler> logger)
    : IRequestHandler<ProcessTelemetryCommand, Result>
{
    /// <summary>Runs the whole ingestion pipeline for one message.</summary>
    public async ValueTask<Result> Handle(ProcessTelemetryCommand request, CancellationToken ct)
    {
        var serial = request.SerialNumber.Trim();
        var device = await db.Devices.FirstOrDefaultAsync(d => d.SerialNumber == serial, ct);

        // Only registered, paired devices are trusted (unknown serials are dropped by the caller).
        if (device?.PatientId is not { } patientId)
        {
            return Result.Failure(ApplicationErrors.Telemetry.UnknownDevice);
        }

        var sample = VitalsSample.Create(patientId, device.Id, request.Timestamp, request.HeartRate, request.Spo2);
        if (sample.IsFailure)
        {
            return Result.Failure(sample.Error);
        }

        var features = EcgFeatureSet.Create(
            patientId, request.Timestamp, request.PrIntervalMs, request.QrsDurationMs, request.QtMs, request.QtcMs,
            request.FeatureVectorJson);
        if (features.IsFailure)
        {
            return Result.Failure(features.Error);
        }

        // Step 1 - keep what the device measured, then show it live.
        await vitals.AppendAsync(sample.Value, ct);
        db.EcgFeatureSets.Add(features.Value);
        device.RecordSeen(clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        await PublishLiveAsync(patientId, request, ct);

        // Step 2 - ask the AI; if it is unavailable the stored data above is still kept.
        var aiResult = await RequestAssessmentAsync(patientId, request, ct);
        if (aiResult.IsFailure)
        {
            logger.LogWarning(
                "No risk assessment for patient {PatientId}: {ErrorCode}.", patientId, aiResult.Error.Code);
            return Result.Success();
        }

        // Step 3 - store the assessment, raise/resolve alerts, then push the outcome.
        var now = clock.GetUtcNow();
        var hasOpenCriticalAlert = await db.Alerts.AnyAsync(
            a => a.PatientId == patientId && a.Severity == AlertSeverity.Critical && a.ResolvedAt == null, ct);
        var previousLevel = await db.RiskAssessments.AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => (RiskStatusLevel?)r.StatusLevel)
            .FirstOrDefaultAsync(ct);

        var modelOutput = aiResult.Value;
        var assessment = RiskAssessment.Create(
            patientId, features.Value.Id, request.Timestamp, modelOutput.StatusLevel, modelOutput.ConfidenceScore,
            modelOutput.ModelVersion, modelOutput.RiskScore, modelOutput.Flags, hasOpenCriticalAlert);
        if (assessment.IsFailure)
        {
            logger.LogWarning(
                "AI answer rejected for patient {PatientId}: {ErrorCode}.", patientId, assessment.Error.Code);
            return Result.Success();
        }

        db.RiskAssessments.Add(assessment.Value);

        var raisedAlert = RaiseAlert(patientId, assessment.Value, previousLevel, hasOpenCriticalAlert, now);
        if (raisedAlert is not null)
        {
            db.Alerts.Add(raisedAlert);
        }

        if (assessment.Value.StatusLevel == RiskStatusLevel.Stable)
        {
            await ResolveOpenRiskAlertsAsync(patientId, now, ct);
        }

        await db.SaveChangesAsync(ct);
        await PublishAssessmentAsync(patientId, assessment.Value, raisedAlert, request, ct);
        return Result.Success();
    }

    /// <summary>Pushes the vitals tick and the live waveform window (neither is persisted by the push).</summary>
    private async Task PublishLiveAsync(Guid patientId, ProcessTelemetryCommand request, CancellationToken ct)
    {
        await notifier.NotifyVitalsAsync(
            new VitalsTickPayload(patientId, request.HeartRate, request.Spo2, request.Timestamp), ct);

        if (request.EcgSamples is { Count: > 0 } samples)
        {
            await notifier.NotifyEcgSamplesAsync(patientId, samples, ct);
        }
    }

    /// <summary>Builds the clinical context and calls the AI service.</summary>
    private async Task<Result<RiskAssessmentResult>> RequestAssessmentAsync(
        Guid patientId, ProcessTelemetryCommand request, CancellationToken ct)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient is null)
        {
            return Result.Failure<RiskAssessmentResult>(ApplicationErrors.Patient.NotFound);
        }

        var diagnoses = await db.MedicalHistoryEntries.AsNoTracking()
            .Where(e => e.PatientId == patientId && e.EntryType != MedicalHistoryEntryType.Medication)
            .Select(e => e.Description)
            .ToListAsync(ct);

        var features = new EcgFeatures(
            request.PrIntervalMs, request.QrsDurationMs, request.QtMs, request.QtcMs, request.HeartRate, request.Spo2);
        var context = new PatientContext(patient.GetAge(DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime)),
            diagnoses);

        return await ai.AssessAsync(new RiskAssessmentRequest(patientId, features, context), ct);
    }

    /// <summary>
    /// Decides whether this assessment raises an alert: Critical when none is open yet, Moderate when the
    /// status worsens from Stable (or from nothing) to Caution; otherwise no alert, so readings never spam.
    /// The message is patient-visible, so it never carries the doctor-only AI flags (those travel in the
    /// clinical riskStatus push instead).
    /// </summary>
    private static Alert? RaiseAlert(
        Guid patientId, RiskAssessment assessment, RiskStatusLevel? previousLevel, bool hasOpenCriticalAlert,
        DateTimeOffset now)
    {
        return assessment.StatusLevel switch
        {
            RiskStatusLevel.Critical when !hasOpenCriticalAlert => Alert.Create(
                patientId, AlertType.RiskThreshold, AlertSeverity.Critical,
                "Critical cardiac risk detected.", now),
            RiskStatusLevel.Caution when previousLevel is null or RiskStatusLevel.Stable => Alert.Create(
                patientId, AlertType.RiskThreshold, AlertSeverity.Moderate, "Cardiac risk needs attention.",
                now),
            _ => null
        };
    }

    /// <summary>
    /// Auto-resolves open non-critical risk alerts once the patient is Stable again (critical ones need a doctor).
    /// </summary>
    private async Task ResolveOpenRiskAlertsAsync(Guid patientId, DateTimeOffset now, CancellationToken ct)
    {
        var open = await db.Alerts
            .Where(a => a.PatientId == patientId
                        && a.ResolvedAt == null
                        && a.Type == AlertType.RiskThreshold
                        && a.Severity != AlertSeverity.Critical)
            .ToListAsync(ct);

        foreach (var alert in open)
        {
            var resolved = alert.Resolve("Status returned to Stable automatically.", resolvedByDoctor: false, now);
            if (resolved.IsFailure)
            {
                logger.LogWarning("Alert {AlertId} could not be auto-resolved: {ErrorCode}.", alert.Id,
                resolved.Error.Code);
            }
        }
    }

    /// <summary>Pushes the new status (two audience variants), the new alert and the doctors' list summaries.</summary>
    private async Task PublishAssessmentAsync(
        Guid patientId, RiskAssessment assessment, Alert? raisedAlert, ProcessTelemetryCommand request,
        CancellationToken ct)
    {
        var clinical = new RiskStatusPayload(
            patientId, assessment.StatusLevel, assessment.ConfidenceScore, assessment.RiskScore, assessment.Flags,
            assessment.Timestamp);
        var patientView = new RiskStatusPayload(patientId, assessment.StatusLevel, null, null, [],
            assessment.Timestamp);
        await notifier.NotifyRiskStatusAsync(new RiskStatusBroadcast(clinical, patientView), ct);

        if (raisedAlert is not null)
        {
            await notifier.NotifyAlertAsync(raisedAlert.ToDto(), ct);
        }

        var doctorIds = await db.DoctorPatientAssignments.AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .Select(a => a.DoctorId)
            .ToListAsync(ct);

        if (doctorIds.Count > 0)
        {
            var summary = new PatientSummaryUpdate(
                patientId,
                assessment.StatusLevel,
                StatusLabels.For(assessment.StatusLevel, true),
                request.HeartRate,
                request.Spo2,
                true);
            await notifier.NotifyPatientSummaryAsync(doctorIds, summary, ct);
        }
    }
}
