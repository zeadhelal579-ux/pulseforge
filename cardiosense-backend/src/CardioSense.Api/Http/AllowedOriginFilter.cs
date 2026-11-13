// =============================================================================
// File           : AllowedOriginFilter.cs
// Layer          : Api / Http
// Purpose        : Blocks cookie-driven auth calls that were triggered from a site that is not the web client.
// Logic          : /api/auth/refresh and /logout act on the httpOnly refresh cookie, which a browser attaches
//                  automatically - even when another website makes the request (CSRF). Browsers always send the
//                  Origin header on such cross-site POSTs, so an Origin that is not on the CORS allow-list is
//                  rejected. Requests with no Origin (curl, server-to-server, same-origin GET) are not browser
//                  cross-site requests and pass.
// Implementation : Endpoint filter reusing the same allow-list as CORS, so the two can never drift apart.
// =============================================================================
using CardioSense.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace CardioSense.Api.Http;

/// <summary>Rejects requests whose Origin header is not an allowed web-client origin.</summary>
internal sealed class AllowedOriginFilter(IOptions<ApiCorsOptions> corsOptions) : IEndpointFilter
{
    /// <summary>Checks the Origin header, then continues to the endpoint when it is absent or allowed.</summary>
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var origin = context.HttpContext.Request.Headers[HeaderNames.Origin].ToString();

        if (origin.Length > 0 && !corsOptions.Value.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult<object?>(ApiErrors.OriginNotAllowed.ToProblem());
        }

        return next(context);
    }
}
