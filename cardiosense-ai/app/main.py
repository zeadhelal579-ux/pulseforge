"""
File           : main.py
Layer          : AI service
Purpose        : The FastAPI application: /health, /analyze and /report.
Logic          : This service is INTERNAL. It must never be reachable from the public internet: it has no notion of
                 who is logged in and trusts the .NET backend to have authorized whatever led to the call (the docker
                 network keeps it private). It is a thin translation layer: validate JSON, convert to plain
                 dataclasses, call the model, convert back. All decisions live in app/models and app/report.
Implementation : Plain synchronous endpoints (the work is CPU-trivial); responses use camelCase aliases.
"""
from __future__ import annotations

from fastapi import FastAPI

from app import report
from app.inference import get_classifier
from app.models import risk_classifier as model
from app.schemas import (
    AnalyzeRequest,
    AnalyzeResponse,
    HealthResponse,
    ReportRequest,
    ReportResponse,
)
from app.schemas import PatientContext as PatientContextBody

app = FastAPI(
    title="CardioSense AI service (research prototype)",
    description="Rule-based risk assessment and report text. Not clinically validated.",
    version="0.1.0",
)


def _to_context(context: PatientContextBody) -> model.PatientContext:
    """Converts the request's patient context to the model's dataclass."""
    return model.PatientContext(age=context.age, prior_diagnosis=tuple(context.prior_diagnosis))


@app.get("/health", response_model=HealthResponse, response_model_by_alias=True)
def health() -> HealthResponse:
    """Liveness probe (also tells which model version is serving)."""
    return HealthResponse(status="ok", model_version=get_classifier().model_version)


@app.post("/analyze", response_model=AnalyzeResponse, response_model_by_alias=True)
def analyze(request: AnalyzeRequest) -> AnalyzeResponse:
    """Classifies one reading."""
    qt_ms, qtc_ms = request.features.qt_qtc_ms
    features = model.Features(
        pr_interval_ms=request.features.pr_interval_ms,
        qrs_duration_ms=request.features.qrs_duration_ms,
        qt_ms=qt_ms,
        qtc_ms=qtc_ms,
        heart_rate=request.features.heart_rate,
        spo2=request.features.spo2,
    )

    assessment = get_classifier().assess(features, _to_context(request.patient_context))

    return AnalyzeResponse(
        status_level=assessment.status_level,
        confidence_score=assessment.confidence_score,
        model_version=assessment.model_version,
        risk_score=assessment.risk_score,
        flags=list(assessment.flags),
    )


@app.post("/report", response_model=ReportResponse, response_model_by_alias=True)
def create_report(request: ReportRequest) -> ReportResponse:
    """Writes the narrative for a report period from the supplied statistics."""
    statistics = report.ReportStatistics(
        average_heart_rate=request.statistics.average_heart_rate,
        highest_risk_score=request.statistics.highest_risk_score,
        alerts_logged_count=request.statistics.alerts_logged_count,
    )

    narrative = report.build_narrative(request.period, _to_context(request.patient_context), statistics)
    return ReportResponse(narrative_summary=narrative)
