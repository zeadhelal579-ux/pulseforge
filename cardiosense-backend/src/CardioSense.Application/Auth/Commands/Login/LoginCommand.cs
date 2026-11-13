// =============================================================================
// File           : LoginCommand.cs
// Layer          : Application / Auth/Commands/Login
// Purpose        : Request to sign in with e-mail and password.
// Logic          : Body of POST /api/auth/login; the result is an AuthSession the Api converts to a JSON body
//                  plus a refresh cookie.
// Implementation : Immutable record implementing the Mediator request contract.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Auth.Commands.Login;

/// <summary>Sign in.</summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthSession>>;
