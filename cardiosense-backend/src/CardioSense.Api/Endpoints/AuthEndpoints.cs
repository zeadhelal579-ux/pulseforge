// =============================================================================
// File           : AuthEndpoints.cs
// Layer          : Api / Endpoints
// Purpose        : /api/auth - sign up (patient or doctor), log in, refresh the session, log out.
// Logic          : The Application returns an AuthSession holding both tokens. The endpoint splits it: the raw refresh
//                  token goes into the httpOnly cookie, everything else into the JSON body (the access token is kept
//                  in memory by the frontend). Refresh and logout act on the cookie, so they carry the origin check;
//                  login and sign-up are rate limited because they are the guessable endpoints. A refresh that is
//                  rejected also clears the cookie, so a dead token is not sent again.
// Implementation : Anonymous endpoints; each handler is one call to the mediator plus the cookie handling.
// =============================================================================
using CardioSense.Api.Authentication;
using CardioSense.Api.Http;
using CardioSense.Application.Auth.Commands.Login;
using CardioSense.Application.Auth.Commands.Logout;
using CardioSense.Application.Auth.Commands.RefreshToken;
using CardioSense.Application.Auth.Commands.RegisterDoctor;
using CardioSense.Application.Auth.Commands.RegisterPatient;
using CardioSense.Application.Auth.Common;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Api.Endpoints;

/// <summary>Authentication and session endpoints.</summary>
internal sealed class AuthEndpoints : IEndpointGroup
{
    /// <summary>Maps the auth routes.</summary>
    public void Map(IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth").AddEndpointFilter<AllowedOriginFilter>();

        auth.MapPost("/signup/patient", SignUpPatient).RequireRateLimiting(ApiRoutes.AuthRateLimit);
        auth.MapPost("/signup/doctor", SignUpDoctor).RequireRateLimiting(ApiRoutes.AuthRateLimit);
        auth.MapPost("/login", Login).RequireRateLimiting(ApiRoutes.AuthRateLimit);
        auth.MapPost("/refresh", Refresh);
        auth.MapPost("/logout", Logout);
    }

    /// <summary>POST /api/auth/signup/patient - creates the account and signs the patient in.</summary>
    private static async Task<IResult> SignUpPatient(
        RegisterPatientCommand command, ISender sender, RefreshCookie cookie, HttpContext http, CancellationToken ct) =>
        StartSession(await sender.Send(command, ct), cookie, http);

    /// <summary>POST /api/auth/signup/doctor - creates the account and signs the doctor in.</summary>
    private static async Task<IResult> SignUpDoctor(
        RegisterDoctorCommand command, ISender sender, RefreshCookie cookie, HttpContext http, CancellationToken ct) =>
        StartSession(await sender.Send(command, ct), cookie, http);

    /// <summary>POST /api/auth/login - checks the credentials and signs the user in.</summary>
    private static async Task<IResult> Login(
        LoginCommand command, ISender sender, RefreshCookie cookie, HttpContext http, CancellationToken ct) =>
        StartSession(await sender.Send(command, ct), cookie, http);

    /// <summary>POST /api/auth/refresh - rotates the refresh cookie and returns a new access token.</summary>
    private static async Task<IResult> Refresh(
        ISender sender, RefreshCookie cookie, HttpContext http, CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenCommand(cookie.Read(http.Request)), ct);

        if (result.IsFailure)
        {
            if (result.Error.Type == ErrorType.Unauthorized)
            {
                cookie.Clear(http.Response);
            }

            return result.Error.ToProblem();
        }

        cookie.Write(http.Response, result.Value.RawRefreshToken, result.Value.RefreshTokenExpiresAt);
        return TypedResults.Ok(result.Value.ToRefreshResponse());
    }

    /// <summary>POST /api/auth/logout - revokes the refresh token and always clears the cookie.</summary>
    private static async Task<IResult> Logout(
        ISender sender, RefreshCookie cookie, HttpContext http, CancellationToken ct)
    {
        var result = await sender.Send(new LogoutCommand(cookie.Read(http.Request)), ct);
        cookie.Clear(http.Response);
        return result.ToNoContentResult();
    }

    /// <summary>Writes the refresh cookie and returns the JSON body, or the problem response on failure.</summary>
    private static IResult StartSession(Result<AuthSession> result, RefreshCookie cookie, HttpContext http)
    {
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        cookie.Write(http.Response, result.Value.RawRefreshToken, result.Value.RefreshTokenExpiresAt);
        return TypedResults.Ok(result.Value.ToResponse());
    }
}
