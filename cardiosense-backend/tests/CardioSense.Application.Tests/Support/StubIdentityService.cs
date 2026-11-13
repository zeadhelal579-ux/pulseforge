// =============================================================================
// File           : StubIdentityService.cs
// Layer          : Tests / Support
// Purpose        : IIdentityService over a dictionary, in place of ASP.NET Core Identity.
// Logic          : Registration and login handlers only need "create an account", "check a password" and "find an
//                  account"; the stub keeps passwords in plain text because it never leaves the test process.
// Implementation : Case-insensitive unique email, same Error shapes as the real service.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;

namespace CardioSense.Application.Tests.Support;

/// <summary>An in-memory account store.</summary>
public sealed class StubIdentityService : IIdentityService
{
    /// <summary>Accounts and their passwords, by user id.</summary>
    private readonly Dictionary<Guid, (UserAccount Account, string Password)> _users = [];

    /// <summary>Adds an account directly (for tests that need an existing user).</summary>
    public void Add(UserAccount account, string password) => _users[account.UserId] = (account, password);

    /// <inheritdoc />
    public Task<Result<Guid>> CreateUserAsync(NewUser user, CancellationToken ct = default)
    {
        var emailTaken = _users.Values.Any(entry =>
            string.Equals(entry.Account.Email, user.Email, StringComparison.OrdinalIgnoreCase));
        if (emailTaken)
        {
            var conflict = Error.Conflict("Auth.EmailTaken", "Email already registered.");
            return Task.FromResult(Result.Failure<Guid>(conflict));
        }

        var userId = Guid.NewGuid();
        Add(new UserAccount(userId, user.Email, user.Role, user.PreferredLanguage), user.Password);
        return Task.FromResult(Result.Success(userId));
    }

    /// <inheritdoc />
    public Task<Result<UserAccount>> ValidateCredentialsAsync(
        string email, string password, CancellationToken ct = default)
    {
        var account = _users.Values
            .Where(entry => entry.Account.Email == email && entry.Password == password)
            .Select(entry => entry.Account)
            .FirstOrDefault();

        return Task.FromResult(account is null
            ? Result.Failure<UserAccount>(ApplicationErrors.Auth.InvalidCredentials)
            : Result.Success(account));
    }

    /// <inheritdoc />
    public Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(_users.TryGetValue(userId, out var entry) ? entry.Account : null);

    /// <inheritdoc />
    public Task<Result> UpdatePreferredLanguageAsync(Guid userId, string languageCode, CancellationToken ct = default)
    {
        if (!_users.TryGetValue(userId, out var entry))
        {
            return Task.FromResult(Result.Failure(Error.NotFound("Auth.UserNotFound", "User was not found.")));
        }

        _users[userId] = (entry.Account with { PreferredLanguage = languageCode }, entry.Password);
        return Task.FromResult(Result.Success());
    }
}
