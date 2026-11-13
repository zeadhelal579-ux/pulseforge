// =============================================================================
// File           : HttpRiskAssessmentServiceTests.cs
// Layer          : Tests / AiClient
// Purpose        : Verifies the HTTP client for the Python AI service against a fake HTTP handler (no network).
// Logic          : The client's safety rule: it never invents an answer. Anything unusable - the service down, an
//                  error status, a body that is not JSON, a missing or unknown status, an integer where a status
//                  name belongs - becomes an Unavailable error, so the caller keeps the data and skips the assessment
//                  instead of defaulting a patient to "Stable".
// Implementation : A stub HttpMessageHandler returns scripted responses and records the request it received.
// =============================================================================
using System.Net;
using System.Text;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using CardioSense.Infrastructure.AiClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace CardioSense.Infrastructure.Tests.AiClient;

/// <summary>Tests for HttpRiskAssessmentService.</summary>
public sealed class HttpRiskAssessmentServiceTests
{
    /// <summary>A complete, valid /analyze answer.</summary>
    private const string ValidAnswer =
        """{"statusLevel":"Critical","confidenceScore":0.8,"modelVersion":"rules-v0",""" +
        "\"riskScore\":77,\"flags\":[\"prolonged_qtc\"]}";

    /// <summary>The request every assessment test sends.</summary>
    private static readonly RiskAssessmentRequest Request = new(
        Guid.NewGuid(), new EcgFeatures(152, 88, 396, 418, 74, 98), new PatientContext(58, ["Prior MI"]));

    /// <summary>Builds a JSON response with the given status code.</summary>
    private static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    /// <summary>Creates the service over a handler that answers with the given function.</summary>
    private static HttpRiskAssessmentService ServiceFor(StubHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://ai.test/") },
            NullLogger<HttpRiskAssessmentService>.Instance);

    /// <summary>A valid answer is mapped to an assessment.</summary>
    [Fact]
    public async Task A_valid_answer_is_mapped()
    {
        var service = ServiceFor(new StubHandler(_ => Json(ValidAnswer)));

        var result = await service.AssessAsync(Request);

        Assert.True(result.IsSuccess);
        Assert.Equal(RiskStatusLevel.Critical, result.Value.StatusLevel);
        Assert.Equal(0.8, result.Value.ConfidenceScore);
        Assert.Equal("rules-v0", result.Value.ModelVersion);
        Assert.Equal(77, result.Value.RiskScore);
        Assert.Equal(new[] { "prolonged_qtc" }, result.Value.Flags);
    }

    /// <summary>The request goes to /analyze in the shape the Python service expects.</summary>
    [Fact]
    public async Task The_request_matches_the_ai_contract()
    {
        var handler = new StubHandler(_ => Json(ValidAnswer));

        await ServiceFor(handler).AssessAsync(Request);

        Assert.Equal("/analyze", handler.LastRequest?.RequestUri?.AbsolutePath);
        Assert.Contains("\"qtQtcMs\":[396,418]", handler.LastBody);
        Assert.Contains("\"priorDiagnosis\":[\"Prior MI\"]", handler.LastBody);
        Assert.Contains("\"heartRate\":74", handler.LastBody);
    }

    /// <summary>An answer without a status is unusable, never defaulted to Stable.</summary>
    [Fact]
    public async Task An_answer_without_a_status_is_rejected()
    {
        var service = ServiceFor(new StubHandler(_ => Json("""{"confidenceScore":0.9,"modelVersion":"x"}""")));

        var result = await service.AssessAsync(Request);

        Assert.Equal(ErrorType.Unavailable, result.Error.Type);
        Assert.Equal("Ai.InvalidAnswer", result.Error.Code);
    }

    /// <summary>A status name that is not Stable, Caution or Critical is rejected.</summary>
    [Fact]
    public async Task An_unknown_status_name_is_rejected()
    {
        var service = ServiceFor(new StubHandler(_ => Json(
            """{"statusLevel":"Fine","confidenceScore":0.9,"modelVersion":"x"}""")));

        var result = await service.AssessAsync(Request);

        Assert.Equal("Ai.InvalidAnswer", result.Error.Code);
    }

    /// <summary>A bare number in place of a status name is rejected (0 would mean Stable).</summary>
    [Fact]
    public async Task A_numeric_status_is_rejected()
    {
        var service = ServiceFor(new StubHandler(_ => Json(
            """{"statusLevel":0,"confidenceScore":0.9,"modelVersion":"x"}""")));

        var result = await service.AssessAsync(Request);

        Assert.Equal("Ai.InvalidAnswer", result.Error.Code);
    }

    /// <summary>A body that is not JSON is rejected.</summary>
    [Fact]
    public async Task A_body_that_is_not_json_is_rejected()
    {
        var service = ServiceFor(new StubHandler(_ => Json("<html>oops</html>")));

        var result = await service.AssessAsync(Request);

        Assert.Equal("Ai.InvalidAnswer", result.Error.Code);
    }

    /// <summary>An error status from the service is reported as unavailable.</summary>
    [Fact]
    public async Task An_error_status_means_the_service_is_unavailable()
    {
        var service = ServiceFor(new StubHandler(_ => Json("{}", HttpStatusCode.InternalServerError)));

        var result = await service.AssessAsync(Request);

        Assert.Equal(ErrorType.Unavailable, result.Error.Type);
        Assert.Equal("Ai.Unavailable", result.Error.Code);
    }

    /// <summary>A connection failure is reported as unavailable.</summary>
    [Fact]
    public async Task A_connection_failure_means_the_service_is_unavailable()
    {
        var service = ServiceFor(new StubHandler(_ => throw new HttpRequestException("connection refused")));

        var result = await service.AssessAsync(Request);

        Assert.Equal("Ai.Unavailable", result.Error.Code);
    }

    /// <summary>A narrative is returned trimmed, and the request goes to /report.</summary>
    [Fact]
    public async Task A_report_narrative_is_returned_trimmed()
    {
        var handler = new StubHandler(_ => Json("""{"narrativeSummary":"  Stable day.  "}"""));
        var request = new ReportNarrativeRequest(
            "daily", new PatientContext(58, []), new ReportStatistics(74.2, 31, 2));

        var result = await ServiceFor(handler).GenerateReportNarrativeAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Stable day.", result.Value);
        Assert.Equal("/report", handler.LastRequest?.RequestUri?.AbsolutePath);
        Assert.Contains("\"period\":\"daily\"", handler.LastBody);
        Assert.Contains("\"alertsLoggedCount\":2", handler.LastBody);
    }

    /// <summary>An empty narrative is rejected: a report is never made up.</summary>
    [Fact]
    public async Task An_empty_narrative_is_rejected()
    {
        var service = ServiceFor(new StubHandler(_ => Json("""{"narrativeSummary":"   "}""")));
        var request = new ReportNarrativeRequest(
            "daily", new PatientContext(58, []), new ReportStatistics(null, null, 0));

        var result = await service.GenerateReportNarrativeAsync(request);

        Assert.Equal("Ai.InvalidAnswer", result.Error.Code);
    }

    /// <summary>An HTTP handler that answers with a scripted response and remembers the request.</summary>
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        /// <summary>The last request received.</summary>
        public HttpRequestMessage? LastRequest { get; private set; }

        /// <summary>The body of the last request.</summary>
        public string? LastBody { get; private set; }

        /// <summary>Records the request and returns the scripted response.</summary>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return respond(request);
        }
    }
}
