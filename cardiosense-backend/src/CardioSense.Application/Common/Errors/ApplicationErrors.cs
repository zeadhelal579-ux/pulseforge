// =============================================================================
// File           : ApplicationErrors.cs
// Layer          : Application / Common/Errors
// Purpose        : Catalogue of the errors raised by use cases (handlers, behaviors) - stable codes plus
//                  messages.
// Logic          : Domain rules own DomainErrors; everything that depends on data lookups or the caller (not
//                  found, access denied, conflicts) lives here so each message is defined exactly once.
// Implementation : Static readonly Error instances grouped by area; the Api layer maps Error.Type to an HTTP
//                  status.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Application.Common.Errors;

/// <summary>Errors raised by Application use cases.</summary>
public static class ApplicationErrors
{
    /// <summary>Authentication errors.</summary>
    public static class Auth
    {
        /// <summary>Unknown email or wrong password (deliberately indistinguishable).</summary>
        public static readonly Error InvalidCredentials = Error.Unauthorized(
            "Auth.InvalidCredentials", "Email or password is incorrect.");

        /// <summary>The refresh token is missing, unknown, expired or revoked.</summary>
        public static readonly Error InvalidRefreshToken = Error.Unauthorized(
            "Auth.InvalidRefreshToken", "The refresh token is missing, expired or has been revoked.");

        /// <summary>The account exists but has no patient/doctor profile (data inconsistency).</summary>
        public static readonly Error ProfileMissing = Error.Failure(
            "Auth.ProfileMissing", "The account has no patient or doctor profile.");
    }

    /// <summary>Patient-related errors.</summary>
    public static class Patient
    {
        /// <summary>The patient does not exist.</summary>
        public static readonly Error NotFound = Error.NotFound("Patient.NotFound", "Patient was not found.");

        /// <summary>The caller may not access this patient.</summary>
        public static readonly Error AccessDenied = Error.Forbidden(
            "Patient.AccessDenied", "You are not allowed to access this patient.");

        /// <summary>Another patient already uses this national id.</summary>
        public static readonly Error NationalIdTaken = Error.Conflict(
            "Patient.NationalIdTaken", "A patient with this national id already exists.");

        /// <summary>The patient already has a paired device.</summary>
        public static readonly Error AlreadyHasDevice = Error.Conflict(
            "Patient.AlreadyHasDevice", "This patient already has a paired device.");

        /// <summary>Someone other than the patient tried to edit the profile.</summary>
        public static readonly Error OnlySelfCanEdit = Error.Forbidden(
            "Patient.OnlySelfCanEdit", "Only the patient can edit this profile.");
    }

    /// <summary>Doctor-related errors.</summary>
    public static class Doctor
    {
        /// <summary>The doctor does not exist.</summary>
        public static readonly Error NotFound = Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        /// <summary>The action is reserved for doctors.</summary>
        public static readonly Error DoctorOnly = Error.Forbidden(
            "Doctor.DoctorOnly", "This action is available to doctors only.");

        /// <summary>Another doctor already uses this license number.</summary>
        public static readonly Error LicenseTaken = Error.Conflict(
            "Doctor.LicenseTaken", "A doctor with this license number already exists.");
    }

    /// <summary>Device-related errors.</summary>
    public static class Device
    {
        /// <summary>The device does not exist (or the caller may not know about it).</summary>
        public static readonly Error NotFound = Error.NotFound("Device.NotFound", "Device was not found.");

        /// <summary>A doctor must say which patient the device belongs to.</summary>
        public static readonly Error PatientRequired = Error.Validation(
            "Device.PatientRequired", "A patient id is required to pair a device on behalf of a patient.");
    }

    /// <summary>Alert-related errors.</summary>
    public static class Alert
    {
        /// <summary>The alert does not exist for this patient.</summary>
        public static readonly Error NotFound = Error.NotFound("Alert.NotFound", "Alert was not found.");
    }

    /// <summary>AI report errors.</summary>
    public static class Report
    {
        /// <summary>No data exists for the requested period.</summary>
        public static readonly Error NoData = Error.NotFound(
            "Report.NoData", "There is no data for the requested period.");
    }

    /// <summary>Telemetry ingestion errors.</summary>
    public static class Telemetry
    {
        /// <summary>The serial number is unknown or the device is not paired to a patient.</summary>
        public static readonly Error UnknownDevice = Error.NotFound(
            "Telemetry.UnknownDevice", "The device is unknown or not paired to a patient.");
    }
}
