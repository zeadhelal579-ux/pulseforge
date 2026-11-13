// =============================================================================
// File           : ContractLiterals.cs
// Layer          : Application / Common/Contracts
// Purpose        : The literal string values of the frontend contract (languages, genders, history ranges,
//                  report periods, list filters).
// Logic          : The frontend sends/expects lower-case literals such as 'ar', 'male', '24h', 'daily',
//                  'alert' that cannot be enum member names; they are defined once here together with their
//                  parsing.
// Implementation : Static classes of constants with TryResolve/Parse helpers; validators and handlers share
//                  them (single source of truth).
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Contracts;

/// <summary>String literals shared with the frontend contract.</summary>
public static class ContractLiterals
{
    /// <summary>Supported interface languages.</summary>
    public static class Languages
    {
        /// <summary>Arabic.</summary>
        public const string Arabic = "ar";

        /// <summary>English.</summary>
        public const string English = "en";

        /// <summary>True when the code is a supported language.</summary>
        public static bool IsSupported(string? code) => code is Arabic or English;
    }

    /// <summary>Gender literals.</summary>
    public static class Genders
    {
        /// <summary>Male literal.</summary>
        public const string Male = "male";

        /// <summary>Female literal.</summary>
        public const string Female = "female";

        /// <summary>True when the literal is supported.</summary>
        public static bool IsSupported(string? value) => value is Male or Female;

        /// <summary>Converts a validated literal to the domain enum.</summary>
        public static Gender Parse(string value) => value switch
        {
            Male => Gender.Male,
            Female => Gender.Female,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported gender literal.")
        };

        /// <summary>Converts the domain enum to its contract literal.</summary>
        public static string ToLiteral(Gender gender) => gender == Gender.Male ? Male : Female;
    }

    /// <summary>Vitals history ranges (query parameter of the vitals-history endpoint).</summary>
    public static class VitalsRanges
    {
        /// <summary>Last 24 hours.</summary>
        public const string Day = "24h";

        /// <summary>Last 7 days.</summary>
        public const string Week = "7d";

        /// <summary>Last 30 days.</summary>
        public const string Month = "30d";

        /// <summary>
        /// Resolves a range to its time window and bucket size; the bucket keeps every chart at or under
        /// roughly 300 points (24h/5min, 7d/1h, 30d/6h).
        /// </summary>
        public static bool TryResolve(string? range, out TimeSpan window, out TimeSpan bucket)
        {
            (window, bucket) = range switch
            {
                Day => (TimeSpan.FromHours(24), TimeSpan.FromMinutes(5)),
                Week => (TimeSpan.FromDays(7), TimeSpan.FromHours(1)),
                Month => (TimeSpan.FromDays(30), TimeSpan.FromHours(6)),
                _ => (TimeSpan.Zero, TimeSpan.Zero)
            };

            return window != TimeSpan.Zero;
        }
    }

    /// <summary>AI report periods.</summary>
    public static class ReportPeriods
    {
        /// <summary>Last 24 hours.</summary>
        public const string Daily = "daily";

        /// <summary>Last 7 days.</summary>
        public const string Weekly = "weekly";

        /// <summary>Last 30 days.</summary>
        public const string Monthly = "monthly";

        /// <summary>Resolves a period (case-insensitive) to its time window.</summary>
        public static bool TryResolve(string? period, out TimeSpan window)
        {
            window = period?.ToLowerInvariant() switch
            {
                Daily => TimeSpan.FromHours(24),
                Weekly => TimeSpan.FromDays(7),
                Monthly => TimeSpan.FromDays(30),
                _ => TimeSpan.Zero
            };

            return window != TimeSpan.Zero;
        }
    }

    /// <summary>Status filters of the doctor's patient list.</summary>
    public static class PatientListFilters
    {
        /// <summary>Every patient.</summary>
        public const string All = "all";

        /// <summary>Only stable patients.</summary>
        public const string Stable = "stable";

        /// <summary>Only patients in Caution (shown as "Alert" in the UI).</summary>
        public const string Alert = "alert";

        /// <summary>Only critical patients.</summary>
        public const string Critical = "critical";

        /// <summary>True when the filter is supported.</summary>
        public static bool IsSupported(string? filter) => filter is All or Stable or Alert or Critical;

        /// <summary>Converts a filter to a status level; null means "no filtering".</summary>
        public static RiskStatusLevel? ToLevel(string filter) => filter switch
        {
            Stable => RiskStatusLevel.Stable,
            Alert => RiskStatusLevel.Caution,
            Critical => RiskStatusLevel.Critical,
            _ => null
        };
    }
}
