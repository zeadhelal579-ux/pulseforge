// =============================================================================
// File           : IdentityModels.cs
// Layer          : Application / Common/Models
// Purpose        : Data shapes exchanged with IIdentityService (new account request, account summary).
// Logic          : Keeps ASP.NET Identity types out of the Application layer - only plain records cross the
//                  boundary.
// Implementation : Two small immutable records declared together because they are used only by the identity
//                  contract.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Models;

/// <summary>Request to create a login account.</summary>
public sealed record NewUser(string Email, string Password, UserRole Role, string PreferredLanguage);

/// <summary>Summary of an existing login account.</summary>
public sealed record UserAccount(Guid UserId, string Email, UserRole Role, string PreferredLanguage);
