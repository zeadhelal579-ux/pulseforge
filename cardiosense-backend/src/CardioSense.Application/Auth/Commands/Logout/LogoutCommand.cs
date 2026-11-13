// =============================================================================
// File           : LogoutCommand.cs
// Layer          : Application / Auth/Commands/Logout
// Purpose        : Request to end the current session by revoking its refresh token.
// Logic          : The Api reads the refresh token from the httpOnly cookie and passes the raw value; a
//                  missing cookie is fine.
// Implementation : Immutable record; returns a plain Result because there is nothing to send back.
// =============================================================================
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Auth.Commands.Logout;

/// <summary>Sign out.</summary>
public sealed record LogoutCommand(string? RawRefreshToken) : IRequest<Result>;
