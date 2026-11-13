// =============================================================================
// File           : DomainErrors.cs
// Layer          : Domain / Common
// Purpose        : Catalogue of the errors that entity behaviour can return (stable codes + messages).
// Logic          : One nested class per entity so call sites read DomainErrors.Alert.CriticalRequiresDoctor;
//                  codes are stable identifiers the frontend/tests can rely on.
// Implementation : Static readonly Error instances - allocated once, compared by value.
// =============================================================================
namespace CardioSense.Domain.Common;

/// <summary>Errors raised by domain entities.</summary>
public static class DomainErrors
{
    /// <summary>Errors of the Alert entity.</summary>
    public static class Alert
    {
        /// <summary>A non-doctor tried to resolve a critical alert.</summary>
        public static readonly Error CriticalRequiresDoctor = Error.Forbidden(
            "Alert.CriticalRequiresDoctor", "Critical alerts can only be resolved by a doctor.");

        /// <summary>The alert was already resolved.</summary>
        public static readonly Error AlreadyResolved = Error.Conflict(
            "Alert.AlreadyResolved", "This alert has already been resolved.");
    }

    /// <summary>Errors of the Device entity.</summary>
    public static class Device
    {
        /// <summary>The device belongs to another patient.</summary>
        public static readonly Error AlreadyPaired = Error.Conflict(
            "Device.AlreadyPaired", "This device is already paired to another patient.");
    }

    /// <summary>Errors of the VitalsSample entity.</summary>
    public static class Vitals
    {
        /// <summary>Heart rate outside the plausible sensor window.</summary>
        public static readonly Error HeartRateOutOfRange = Error.Validation(
            "Vitals.HeartRateOutOfRange",
                "Heart rate is outside the plausible range and is treated as a sensor artefact.");

        /// <summary>SpO2 outside the plausible sensor window.</summary>
        public static readonly Error Spo2OutOfRange = Error.Validation(
            "Vitals.Spo2OutOfRange", "SpO2 is outside the plausible range and is treated as a sensor artefact.");
    }

    /// <summary>Errors of the EcgFeatureSet entity.</summary>
    public static class Ecg
    {
        /// <summary>An interval (PR, QRS, QT, QTc) is not a plausible positive duration.</summary>
        public static readonly Error IntervalOutOfRange = Error.Validation(
            "Ecg.IntervalOutOfRange", "ECG intervals must be positive durations in milliseconds.");
    }

    /// <summary>Errors of the RiskAssessment entity.</summary>
    public static class Risk
    {
        /// <summary>Confidence outside 0..1.</summary>
        public static readonly Error ConfidenceOutOfRange = Error.Validation(
            "Risk.ConfidenceOutOfRange", "Confidence score must be between 0 and 1.");

        /// <summary>Risk score outside 0..100.</summary>
        public static readonly Error RiskScoreOutOfRange = Error.Validation(
            "Risk.RiskScoreOutOfRange", "Risk score must be between 0 and 100.");
    }

    /// <summary>Errors of the ElectrodeVerification entity.</summary>
    public static class Electrode
    {
        /// <summary>Confidence outside 0..1.</summary>
        public static readonly Error ConfidenceOutOfRange = Error.Validation(
            "Electrode.ConfidenceOutOfRange", "Confidence score must be between 0 and 1.");
    }
}
