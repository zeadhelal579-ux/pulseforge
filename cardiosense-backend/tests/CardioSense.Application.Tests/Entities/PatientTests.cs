// =============================================================================
// File           : PatientTests.cs
// Layer          : Tests / Entities
// Purpose        : Verifies age handling: the signup form collects an age, the database stores a date of birth.
// Logic          : The form's age is converted to an approximate birth date, and age is always recomputed from the
//                  birth date, so the round trip must return the same age and birthdays must be counted exactly.
// Implementation : Plain xUnit facts; no database.
// =============================================================================
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Tests.Entities;

/// <summary>Tests for the Patient entity.</summary>
public sealed class PatientTests
{
    /// <summary>Creates a patient born on the given date.</summary>
    private static Patient BornOn(DateOnly dateOfBirth) =>
        Patient.Create(
            Guid.NewGuid(), "Ahmed Mostafa", dateOfBirth, Gender.Male, "29001011234567", "01000000000",
            ApplicationTestHost.Start);

    /// <summary>Age from the form survives the round trip through a date of birth.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(58)]
    [InlineData(100)]
    public void The_form_age_round_trips(int age)
    {
        var patient = BornOn(Patient.DateOfBirthFromAge(age, Seed.Today));

        Assert.Equal(age, patient.GetAge(Seed.Today));
    }

    /// <summary>The age changes exactly on the birthday, not the day before.</summary>
    [Fact]
    public void Age_increases_on_the_birthday()
    {
        var patient = BornOn(new DateOnly(1990, 6, 15));

        Assert.Equal(35, patient.GetAge(new DateOnly(2026, 6, 14)));
        Assert.Equal(36, patient.GetAge(new DateOnly(2026, 6, 15)));
    }

    /// <summary>A contact update changes only the supplied fields.</summary>
    [Fact]
    public void A_contact_update_changes_only_what_is_supplied()
    {
        var patient = BornOn(new DateOnly(1990, 6, 15));

        patient.UpdateContact(null, "01222222222");

        Assert.Equal("Ahmed Mostafa", patient.FullName);
        Assert.Equal("01222222222", patient.Phone);
    }
}
