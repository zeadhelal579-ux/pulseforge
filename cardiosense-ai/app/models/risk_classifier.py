"""
File           : risk_classifier.py
Layer          : AI service / models
Purpose        : Turns one set of ECG/vitals features into a status (Stable / Caution / Critical), a 0-100 risk
                 score, flags and a confidence value.
Logic          : This is a TRANSPARENT RULE-BASED PROTOTYPE, not a trained or clinically validated model. Every rule is
                 a threshold on one measurement, listed in RULES below so a clinician can read, challenge and change
                 them. The thresholds are common textbook adult reference limits and are an ASSUMPTION that needs
                 clinical sign-off before anything is relied on. A trained model (see training/) replaces this class
                 later; the rest of the service only depends on `RiskClassifier.assess`.
Implementation : Pure Python with no third-party imports, so it can be unit-tested anywhere. Status comes from the
                 findings only; patient context (age, cardiac history) can raise the score slightly but never changes
                 the status on its own.
"""
from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass

STABLE = "Stable"
CAUTION = "Caution"
CRITICAL = "Critical"

# Score points per finding, and how a finding maps to the score bands.
CAUTION_POINTS = 25
CRITICAL_POINTS = 60
CONTEXT_POINTS_CAP = 10
ELDERLY_AGE = 65
ELDERLY_POINTS = 5
CARDIAC_HISTORY_POINTS = 5

# Confidence: full when every measurement is clearly normal or clearly abnormal, lower when one sits right next to
# a threshold (its absolute "borderline margin" is defined per rule below).
BASE_CONFIDENCE = 0.95
BORDERLINE_PENALTY = 0.15
MIN_CONFIDENCE = 0.5

# Words in a prior diagnosis that indicate known cardiac disease.
CARDIAC_HISTORY_KEYWORDS = (
    "mi", "infarction", "heart failure", "arrhythmia", "fibrillation", "cardiomyopathy", "angina", "stent",
    "bypass", "valve",
)


@dataclass(frozen=True)
class Features:
    """One reading's measurements (intervals in milliseconds)."""

    pr_interval_ms: int
    qrs_duration_ms: int
    qt_ms: int
    qtc_ms: int
    heart_rate: int
    spo2: int


@dataclass(frozen=True)
class PatientContext:
    """Facts about the patient that frame the reading."""

    age: int
    prior_diagnosis: tuple[str, ...] = ()


@dataclass(frozen=True)
class Assessment:
    """The classifier's answer."""

    status_level: str
    confidence_score: float
    model_version: str
    risk_score: int
    flags: tuple[str, ...]


@dataclass(frozen=True)
class Rule:
    """One threshold rule on one measurement.

    `direction` is "high" (value above the limit is abnormal) or "low" (below the limit is abnormal).
    `critical` is None when the rule can only ever produce a caution.
    `margin` is how close (in the measurement's own unit) a value must be to a limit to count as borderline.
    """

    flag: str
    measure: Callable[[Features], float]
    direction: str
    caution: float
    critical: float | None
    margin: float


MODEL_VERSION = "rules-v0.1-prototype"

RULES: tuple[Rule, ...] = (
    Rule("tachycardia", lambda f: f.heart_rate, "high", caution=110, critical=150, margin=5),
    Rule("bradycardia", lambda f: f.heart_rate, "low", caution=50, critical=40, margin=5),
    Rule("low_spo2", lambda f: f.spo2, "low", caution=94, critical=90, margin=1),
    Rule("prolonged_pr", lambda f: f.pr_interval_ms, "high", caution=200, critical=300, margin=10),
    Rule("short_pr", lambda f: f.pr_interval_ms, "low", caution=120, critical=None, margin=10),
    Rule("wide_qrs", lambda f: f.qrs_duration_ms, "high", caution=120, critical=160, margin=5),
    Rule("prolonged_qtc", lambda f: f.qtc_ms, "high", caution=460, critical=500, margin=10),
)


def _crosses(value: float, limit: float, direction: str) -> bool:
    """True when `value` is beyond `limit` in the abnormal direction (the limit itself is still normal)."""
    return value > limit if direction == "high" else value < limit


def _is_borderline(value: float, limit: float, margin: float) -> bool:
    """True when `value` lies within `margin` of `limit`, on either side."""
    return abs(value - limit) <= margin


def _has_cardiac_history(prior_diagnosis: tuple[str, ...]) -> bool:
    """True when a prior diagnosis mentions known cardiac disease (single words match whole words only)."""
    text = " ".join(prior_diagnosis).lower()
    words = set(text.replace(",", " ").replace("-", " ").split())
    return any((keyword in text) if " " in keyword else (keyword in words) for keyword in CARDIAC_HISTORY_KEYWORDS)


class RiskClassifier:
    """Rule-based classifier (see the module docstring for the honest limits of this model)."""

    model_version = MODEL_VERSION

    def assess(self, features: Features, context: PatientContext) -> Assessment:
        """Evaluates every rule and combines the findings into a status, score, flags and confidence."""
        flags: list[str] = []
        points = 0
        worst = STABLE
        borderline = False

        for rule in RULES:
            value = rule.measure(features)
            limits = [rule.caution] if rule.critical is None else [rule.caution, rule.critical]
            borderline = borderline or any(_is_borderline(value, limit, rule.margin) for limit in limits)

            if rule.critical is not None and _crosses(value, rule.critical, rule.direction):
                flags.append(rule.flag)
                points += CRITICAL_POINTS
                worst = CRITICAL
            elif _crosses(value, rule.caution, rule.direction):
                flags.append(rule.flag)
                points += CAUTION_POINTS
                worst = worst if worst == CRITICAL else CAUTION

        score = min(100, points + self._context_points(context))
        return Assessment(worst, self._confidence(borderline), self.model_version, score, tuple(flags))

    @staticmethod
    def _context_points(context: PatientContext) -> int:
        """Extra score for age and known cardiac history; capped so context alone can never decide a status."""
        points = ELDERLY_POINTS if context.age >= ELDERLY_AGE else 0
        if _has_cardiac_history(context.prior_diagnosis):
            points += CARDIAC_HISTORY_POINTS
        return min(points, CONTEXT_POINTS_CAP)

    @staticmethod
    def _confidence(borderline: bool) -> float:
        """A heuristic certainty, NOT a statistical probability: lower when a value sits next to a threshold."""
        return max(MIN_CONFIDENCE, BASE_CONFIDENCE - (BORDERLINE_PENALTY if borderline else 0.0))
