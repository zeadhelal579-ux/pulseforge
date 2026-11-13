// =============================================================================
// File           : LoginHandler.cs
// Layer          : Application / Auth/Commands/Login
// Purpose        : Handles a login: verifies the credentials, then issues a session.
// Logic          : Unknown e-mail and wrong password produce the same error so the endpoint cannot be used to
//                  discover accounts; lockout (too many attempts) comes back from the identity service.
// Implementation : Internal sealed handler; delegates account checks to IIdentityService and the rest to
//                  SessionIssuer.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Auth.Commands.Login;

/// <summary>Handles <see cref="LoginCommand"/>.</summary>
internal sealed class LoginHandler(IIdentityService identity, SessionIssuer sessions)
    : IRequestHandler<LoginCommand, Result<AuthSession>>
{
    /// <summary>Verifies credentials and issues the session.</summary>
    public async ValueTask<Result<AuthSession>> Handle(LoginCommand request, CancellationToken ct)
    {
        var account = await identity.ValidateCredentialsAsync(request.Email.Trim(), request.Password, ct);

        return account.IsFailure
            ? Result.Failure<AuthSession>(account.Error)
            : await sessions.IssueAsync(account.Value, ct);
    }
}
