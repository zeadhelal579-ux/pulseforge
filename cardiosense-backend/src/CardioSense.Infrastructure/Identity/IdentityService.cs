// =============================================================================
// File           : IdentityService.cs
// Layer          : Infrastructure / Identity
// Purpose        : Implements IIdentityService on top of ASP.NET Core Identity (UserManager).
// Logic          : Creates accounts, verifies credentials (with lockout), loads accounts by id and stores the language.
//                  Wrong e-mail and wrong password return the same error, and an unknown e-mail still pays for one
//                  password hash so response time does not reveal which e-mails are registered.
// Implementation : Scoped service. UserManager shares the request's AppDbContext, so account creation joins the
//                  explicit transaction opened by the registration handlers. UserManager has no cancellation
//                  parameter, so each method checks the token before it starts.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace CardioSense.Infrastructure.Identity;

/// <summary>Login account operations backed by ASP.NET Core Identity.</summary>
internal sealed class IdentityService(UserManager<ApplicationUser> users, TimeProvider clock) : IIdentityService
{
    /// <inheritdoc />
    public async Task<Result<Guid>> CreateUserAsync(NewUser user, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var account = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = user.Email,
            Email = user.Email,
            Role = user.Role,
            PreferredLanguage = user.PreferredLanguage,
            CreatedAt = clock.GetUtcNow()
        };

        var created = await users.CreateAsync(account, user.Password);
        return created.Succeeded
            ? Result.Success(account.Id)
            : Result.Failure<Guid>(IdentityErrors.From(created.Errors));
    }

    /// <inheritdoc />
    public async Task<Result<UserAccount>> ValidateCredentialsAsync(
        string email, string password, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var account = await users.FindByEmailAsync(email);
        if (account is null)
        {
            // Pay the same hashing cost as a real check so timing does not reveal unknown e-mails.
            _ = users.PasswordHasher.HashPassword(new ApplicationUser(), password);
            return Result.Failure<UserAccount>(ApplicationErrors.Auth.InvalidCredentials);
        }

        if (await users.IsLockedOutAsync(account))
        {
            return Result.Failure<UserAccount>(IdentityErrors.LockedOut);
        }

        if (!await users.CheckPasswordAsync(account, password))
        {
            await users.AccessFailedAsync(account);
            return Result.Failure<UserAccount>(ApplicationErrors.Auth.InvalidCredentials);
        }

        await users.ResetAccessFailedCountAsync(account);
        return Result.Success(ToAccount(account));
    }

    /// <inheritdoc />
    public async Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var account = await users.FindByIdAsync(userId.ToString());
        return account is null ? null : ToAccount(account);
    }

    /// <inheritdoc />
    public async Task<Result> UpdatePreferredLanguageAsync(
        Guid userId, string languageCode, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var account = await users.FindByIdAsync(userId.ToString());
        if (account is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        account.PreferredLanguage = languageCode;
        var updated = await users.UpdateAsync(account);
        return updated.Succeeded ? Result.Success() : Result.Failure(IdentityErrors.From(updated.Errors));
    }

    /// <summary>Converts the stored account into the Application model.</summary>
    private static UserAccount ToAccount(ApplicationUser account) =>
        new(account.Id, account.Email ?? string.Empty, account.Role, account.PreferredLanguage);
}
