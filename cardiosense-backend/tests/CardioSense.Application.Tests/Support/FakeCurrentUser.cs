// =============================================================================
// File           : FakeCurrentUser.cs
// Layer          : Tests / Support
// Purpose        : Lets a test sign in as a given patient or doctor (or nobody).
// Logic          : Authorization in Application depends only on ICurrentUser, so changing these four properties is
//                  all it takes to test every access rule.
// Implementation : Mutable properties plus three helpers.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Tests.Support;

/// <summary>A controllable current user.</summary>
public sealed class FakeCurrentUser : ICurrentUser
{
    /// <inheritdoc />
    public Guid? UserId { get; set; }

    /// <inheritdoc />
    public UserRole? Role { get; set; }

    /// <inheritdoc />
    public Guid? PatientId { get; set; }

    /// <inheritdoc />
    public Guid? DoctorId { get; set; }

    /// <summary>Acts as a patient with the given profile id.</summary>
    public void SignInAsPatient(Guid patientId)
    {
        SignOut();
        UserId = Guid.NewGuid();
        Role = UserRole.Patient;
        PatientId = patientId;
    }

    /// <summary>Acts as a doctor with the given profile id.</summary>
    public void SignInAsDoctor(Guid doctorId)
    {
        SignOut();
        UserId = Guid.NewGuid();
        Role = UserRole.Doctor;
        DoctorId = doctorId;
    }

    /// <summary>Acts as an anonymous caller.</summary>
    public void SignOut()
    {
        UserId = null;
        Role = null;
        PatientId = null;
        DoctorId = null;
    }
}
