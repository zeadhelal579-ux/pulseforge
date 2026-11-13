"""
File           : report.py
Layer          : AI service
Purpose        : Writes the short narrative shown in the "AI-Assisted Insights (Prototype)" report card.
Logic          : The narrative only restates the numbers it was given (average heart rate, highest risk score, alert
                 count) in plain words. It never invents a finding, a diagnosis or a trend, and it says so. A
                 language model can replace this later; the contract (statistics in, text out) stays the same.
Implementation : Deterministic string building, no third-party imports.
"""
from __future__ import annotations

from dataclasses import dataclass

from app.models.risk_classifier import PatientContext

PERIOD_LABELS = {
    "daily": "the last 24 hours",
    "weekly": "the last 7 days",
    "monthly": "the last 30 days",
}

# Adult resting heart-rate range used only to put the average in words (same reference as the web client).
TYPICAL_HEART_RATE = (60, 100)

DISCLAIMER = (
    "This summary only restates recorded measurements. It is a research prototype, not a clinical assessment."
)


@dataclass(frozen=True)
class ReportStatistics:
    """Numbers the backend computed for the period (None when there is no data for that number)."""

    average_heart_rate: float | None
    highest_risk_score: float | None
    alerts_logged_count: int


def build_narrative(period: str, context: PatientContext, statistics: ReportStatistics) -> str:
    """Builds the narrative text for a report period."""
    window = PERIOD_LABELS.get(period, period)
    sentences = [f"Summary for {window} (AI-Assisted Insights, prototype)."]

    if context.prior_diagnosis:
        sentences.append(f"Patient age {context.age}; recorded history: {', '.join(context.prior_diagnosis)}.")
    else:
        sentences.append(f"Patient age {context.age}.")

    if statistics.average_heart_rate is not None:
        sentences.append(_heart_rate_sentence(statistics.average_heart_rate))
    if statistics.highest_risk_score is not None:
        sentences.append(f"The highest risk score reached was {statistics.highest_risk_score:.0f} out of 100.")

    sentences.append(_alerts_sentence(statistics.alerts_logged_count))
    sentences.append(DISCLAIMER)
    return " ".join(sentences)


def _heart_rate_sentence(average: float) -> str:
    """Average heart rate with a plain-language position against the typical resting range."""
    low, high = TYPICAL_HEART_RATE
    if average < low:
        position = "below the typical adult resting range"
    elif average > high:
        position = "above the typical adult resting range"
    else:
        position = "within the typical adult resting range"
    return f"The average heart rate was {average:.1f} bpm, {position} ({low}-{high} bpm)."


def _alerts_sentence(count: int) -> str:
    """Alert count in words."""
    if count == 0:
        return "No alerts were logged."
    return f"{count} alert was logged." if count == 1 else f"{count} alerts were logged."
