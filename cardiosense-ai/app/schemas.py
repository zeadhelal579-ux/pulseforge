"""
File           : schemas.py
Layer          : AI service
Purpose        : The JSON contract with the .NET backend (HttpRiskAssessmentService), as Pydantic models.
Logic          : The backend sends and expects camelCase, Python uses snake_case; an alias generator maps between the
                 two so the code stays Pythonic and the wire format stays exactly what AiContracts.cs defines.
                 Ranges are wide on purpose (genuinely abnormal values must pass); they only reject nonsense.
                 The response status is a Literal, so the service can never emit a value the backend would reject.
Implementation : Pydantic v2 models with `alias_generator=to_camel`.
"""
from __future__ import annotations

from typing import Literal
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field
from pydantic.alias_generators import to_camel

StatusLevel = Literal["Stable", "Caution", "Critical"]
ReportPeriod = Literal["daily", "weekly", "monthly"]


class CamelModel(BaseModel):
    """Base class: camelCase on the wire, snake_case in Python."""

    # protected_namespaces=() because the contract has a field called model_version (pydantic reserves model_*).
    model_config = ConfigDict(alias_generator=to_camel, populate_by_name=True, protected_namespaces=())


class Features(CamelModel):
    """Measurements of one reading; qtQtcMs is [QT, QTc]."""

    pr_interval_ms: int = Field(gt=0, le=2000)
    qrs_duration_ms: int = Field(gt=0, le=2000)
    qt_qtc_ms: list[int] = Field(min_length=2, max_length=2)
    heart_rate: int = Field(gt=0, le=400)
    spo2: int = Field(ge=0, le=100)


class PatientContext(CamelModel):
    """Facts about the patient."""

    age: int = Field(ge=0, le=130)
    prior_diagnosis: list[str] = Field(default_factory=list)


class AnalyzeRequest(CamelModel):
    """POST /analyze body."""

    patient_id: UUID
    features: Features
    patient_context: PatientContext


class AnalyzeResponse(CamelModel):
    """POST /analyze answer; status, confidence and model version are always present."""

    status_level: StatusLevel
    confidence_score: float = Field(ge=0, le=1)
    model_version: str
    risk_score: int = Field(ge=0, le=100)
    flags: list[str]


class ReportStatistics(CamelModel):
    """Numbers for the report period (null when there is no data)."""

    average_heart_rate: float | None = None
    highest_risk_score: float | None = None
    alerts_logged_count: int = Field(ge=0)


class ReportRequest(CamelModel):
    """POST /report body."""

    period: ReportPeriod
    patient_context: PatientContext
    statistics: ReportStatistics


class ReportResponse(CamelModel):
    """POST /report answer."""

    narrative_summary: str


class HealthResponse(CamelModel):
    """GET /health answer."""

    status: Literal["ok"]
    model_version: str
