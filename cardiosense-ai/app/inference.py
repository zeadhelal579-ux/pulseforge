"""
File           : inference.py
Layer          : AI service
Purpose        : The seam between the web layer (FastAPI) and the models.
Logic          : main.py never imports a concrete model. It asks this module for a classifier, so swapping the rule
                 prototype for a trained model (loaded once at start-up from a model file) changes this file only.
Implementation : A cached factory returning the single classifier instance used by the process.
"""
from __future__ import annotations

from functools import lru_cache

from app.models.risk_classifier import RiskClassifier


@lru_cache(maxsize=1)
def get_classifier() -> RiskClassifier:
    """Returns the process-wide classifier (created on first use)."""
    return RiskClassifier()
