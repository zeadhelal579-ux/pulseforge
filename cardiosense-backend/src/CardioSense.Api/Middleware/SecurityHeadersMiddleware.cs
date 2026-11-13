// =============================================================================
// File           : SecurityHeadersMiddleware.cs
// Layer          : Api / Middleware
// Purpose        : Adds the response headers every API answer should carry.
// Logic          : This API only returns JSON about real patients, so it must never be cached by a browser or a proxy
//                  (Cache-Control: no-store), must not be sniffed into another content type, and must not leak the
//                  URL as a referrer.
// Implementation : Headers are set in OnStarting so they are added to every response, including error responses
//                  produced by later middleware.
// =============================================================================
using Microsoft.Net.Http.Headers;

namespace CardioSense.Api.Middleware;

/// <summary>Adds no-store, no-sniff and no-referrer headers.</summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>Registers the header callback, then continues down the pipeline.</summary>
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var headers = ((HttpContext)state).Response.Headers;
            headers[HeaderNames.CacheControl] = "no-store";
            headers[HeaderNames.XContentTypeOptions] = "nosniff";
            headers["Referrer-Policy"] = "no-referrer";
            return Task.CompletedTask;
        }, context);

        return next(context);
    }
}
