// =============================================================================
// File           : IIdentityService.cs
// Layer          : Application / Common/Interfaces
// Purpose        : Account management contract (create account, verify credentials, read account, change
//                  language).
// Logic          : Hides ASP.NET Core Identity: passwords, lockout and the Users table stay in
//                  Infrastructure; the Application only sees plain records and Results (email already used,
//                  wrong password, locked out, ...).
// Implementation : Implemented by IdentityService in Infrastructure using UserManager on the same scoped
//                  DbContext, so a surrounding IAppDbContext transaction also covers account creation.
// =============================================================================
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;

namespace CardioSense.Application.Common.Interfaces;

/// <summary>Login account operations.</summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a login account; fails with a Conflict when the e-mail exists or a Validation error for weak passwords.
    /// </summary>
    Task<Result<Guid>> CreateUserAsync(NewUser user, CancellationToken ct = default);

    /// <summary>Verifies credentials; fails with Unauthorized (wrong data) or Forbidden (locked out).</summary>
    Task<Result<UserAccount>> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Finds an account by id; null when it does not exist.</summary>
    Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Stores the account's preferred language ('ar' or 'en').</summary>
    Task<Result> UpdatePreferredLanguageAsync(Guid userId, string languageCode, CancellationToken ct = default);
}
