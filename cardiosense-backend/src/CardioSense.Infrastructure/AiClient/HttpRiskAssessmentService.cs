// =============================================================================
// File           : HttpRiskAssessmentService.cs
// Layer          : Infrastructure / AiClient
// Purpose        : Implements IRiskAssessmentService by calling the Python AI service over internal HTTP/JSON.
// Logic          : AssessAsync -> POST /analyze, GenerateReportNarrativeAsync -> POST /report. Any transport problem,
//                  non-success status, malformed body or incomplete answer becomes an Unavailable error - the caller
//                  decides what to do (the telemetry handler keeps the stored data; the report endpoint reports the
//                  failure). A report is never invented.
// Implementation : Typed HttpClient (base address and timeout come from AiServiceOptions in DependencyInjection).
//                  One generic private method performs every call so error handling exists exactly once.
// =============================================================================
using System.Net.Http.Json;
using System.Text.Json;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using Microsoft.Extensions.Logging;

namespace CardioSense.Infrastructure.AiClient;

/// <summary>HTTP client of the AI service.</summary>
internal sealed class HttpRiskAssessmentService(HttpClient http, ILogger<HttpRiskAssessmentService> logger)
    : IRiskAssessmentService
{
    /// <summary>Relative path of the risk assessment endpoint.</summary>
    private const string AnalyzePath = "analyze";

    /// <summary>Relative path of the report narrative endpoint.</summary>
    private const string ReportPath = "report";

    /// <inheritdoc />
    public async Task<Result<RiskAssessmentResult>> AssessAsync(
        RiskAssessmentRequest request, CancellationToken ct = default)
    {
        var features = request.Features;
        var body = new AnalyzeRequestBody(
            request.PatientId,
            new AiFeatures(
                features.PrIntervalMs, features.QrsDurationMs, [features.QtMs, features.QtcMs], features.HeartRate,
                features.Spo2),
            ToContext(request.Context));

        var answer = await PostAsync<AnalyzeRequestBody, AnalyzeResponseBody>(AnalyzePath, body, ct);
        return answer.IsFailure ? Result.Failure<RiskAssessmentResult>(answer.Error) : ToAssessment(answer.Value);
    }

    /// <inheritdoc />
    public async Task<Result<string>> GenerateReportNarrativeAsync(
        ReportNarrativeRequest request, CancellationToken ct = default)
    {
        var statistics = request.Statistics;
        var body = new ReportRequestBody(
            request.Period,
            ToContext(request.Context),
            new AiReportStatistics(
                statistics.AverageHeartRate, statistics.HighestRiskScore, statistics.AlertsLoggedCount));

        var answer = await PostAsync<ReportRequestBody, ReportResponseBody>(ReportPath, body, ct);
        if (answer.IsFailure)
        {
            return Result.Failure<string>(answer.Error);
        }

        if (string.IsNullOrWhiteSpace(answer.Value.NarrativeSummary))
        {
            logger.LogWarning("AI service returned an empty report narrative.");
            return Result.Failure<string>(AiErrors.InvalidAnswer);
        }

        return Result.Success(answer.Value.NarrativeSummary.Trim());
    }

    /// <summary>Sends one JSON request and reads the JSON answer; every failure becomes an Unavailable error.</summary>
    private async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
        string path, TRequest body, CancellationToken ct)
        where TResponse : class
    {
        try
        {
            using var response = await http.PostAsJsonAsync(path, body, AiJson.Options, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AI service /{Path} answered HTTP {StatusCode}.", path, (int)response.StatusCode);
                return Result.Failure<TResponse>(AiErrors.Unavailable);
            }

            var answer = await response.Content.ReadFromJsonAsync<TResponse>(AiJson.Options, ct);
            return answer is null ? Result.Failure<TResponse>(AiErrors.InvalidAnswer) : Result.Success(answer);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "AI service /{Path} could not be reached.", path);
            return Result.Failure<TResponse>(AiErrors.Unavailable);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("AI service /{Path} timed out.", path);
            return Result.Failure<TResponse>(AiErrors.Unavailable);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            logger.LogWarning(exception, "AI service /{Path} returned a body that is not valid JSON.", path);
            return Result.Failure<TResponse>(AiErrors.InvalidAnswer);
        }
    }

    /// <summary>Converts the answer to the Application model, rejecting incomplete answers.</summary>
    private Result<RiskAssessmentResult> ToAssessment(AnalyzeResponseBody answer)
    {
        if (answer.StatusLevel is not { } statusLevel
            || answer.ConfidenceScore is not { } confidenceScore
            || string.IsNullOrWhiteSpace(answer.ModelVersion))
        {
            logger.LogWarning("AI service returned an incomplete assessment.");
            return Result.Failure<RiskAssessmentResult>(AiErrors.InvalidAnswer);
        }

        return Result.Success(new RiskAssessmentResult(
            statusLevel, confidenceScore, answer.ModelVersion, answer.RiskScore, answer.Flags ?? []));
    }

    /// <summary>Converts the Application context to the wire context.</summary>
    private static AiPatientContext ToContext(PatientContext context) => new(context.Age, context.PriorDiagnoses);
}
