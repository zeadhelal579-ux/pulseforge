"""
File           : test_risk_classifier.py
Layer          : AI service / tests
Purpose        : Pins down the rule-based classifier: which readings are Stable, Caution or Critical, and why.
Logic          : The thresholds are the clinically sensitive part of this prototype, so each rule is tested on both
                 sides of its limit. Also tested: the safety properties - context alone never changes a status, the
                 score stays in 0..100, and the model never reports a status without saying which flags caused it.
Implementation : Standard-library unittest (no third-party packages needed): `python -m unittest discover -s tests`.
"""
import unittest

from app.models.risk_classifier import (
    CAUTION,
    CRITICAL,
    STABLE,
    Features,
    PatientContext,
    RiskClassifier,
)

NORMAL = Features(pr_interval_ms=160, qrs_duration_ms=90, qt_ms=380, qtc_ms=410, heart_rate=75, spo2=98)
ADULT = PatientContext(age=40)


def reading(**changes: int) -> Features:
    """A normal reading with some measurements replaced."""
    values = {**NORMAL.__dict__, **changes}
    return Features(**values)


class RiskClassifierTests(unittest.TestCase):
    """Tests for RiskClassifier.assess."""

    def setUp(self) -> None:
        self.classifier = RiskClassifier()

    def assess(self, features: Features, context: PatientContext = ADULT):
        """Shortcut for the classifier call."""
        return self.classifier.assess(features, context)

    def test_a_normal_reading_is_stable_with_no_flags(self) -> None:
        result = self.assess(NORMAL)

        self.assertEqual(STABLE, result.status_level)
        self.assertEqual((), result.flags)
        self.assertEqual(0, result.risk_score)
        self.assertEqual(0.95, result.confidence_score)
        self.assertTrue(result.model_version.startswith("rules-"))

    def test_the_limits_themselves_are_still_normal(self) -> None:
        for field, limit in (("heart_rate", 110), ("heart_rate", 50), ("spo2", 94), ("pr_interval_ms", 200),
                             ("pr_interval_ms", 120), ("qrs_duration_ms", 120), ("qtc_ms", 460)):
            with self.subTest(field=field, limit=limit):
                self.assertEqual(STABLE, self.assess(reading(**{field: limit})).status_level)

    def test_fast_heart_rate(self) -> None:
        caution = self.assess(reading(heart_rate=120))
        critical = self.assess(reading(heart_rate=160))

        self.assertEqual((CAUTION, ("tachycardia",)), (caution.status_level, caution.flags))
        self.assertEqual((CRITICAL, ("tachycardia",)), (critical.status_level, critical.flags))

    def test_slow_heart_rate(self) -> None:
        caution = self.assess(reading(heart_rate=45))
        critical = self.assess(reading(heart_rate=35))

        self.assertEqual((CAUTION, ("bradycardia",)), (caution.status_level, caution.flags))
        self.assertEqual((CRITICAL, ("bradycardia",)), (critical.status_level, critical.flags))

    def test_low_oxygen_saturation(self) -> None:
        caution = self.assess(reading(spo2=92))
        critical = self.assess(reading(spo2=88))

        self.assertEqual((CAUTION, ("low_spo2",)), (caution.status_level, caution.flags))
        self.assertEqual((CRITICAL, ("low_spo2",)), (critical.status_level, critical.flags))

    def test_pr_interval_too_long_or_too_short(self) -> None:
        long = self.assess(reading(pr_interval_ms=220))
        very_long = self.assess(reading(pr_interval_ms=320))
        short = self.assess(reading(pr_interval_ms=100))

        self.assertEqual((CAUTION, ("prolonged_pr",)), (long.status_level, long.flags))
        self.assertEqual(CRITICAL, very_long.status_level)
        self.assertEqual((CAUTION, ("short_pr",)), (short.status_level, short.flags))

    def test_a_short_pr_interval_alone_is_never_critical(self) -> None:
        self.assertEqual(CAUTION, self.assess(reading(pr_interval_ms=60)).status_level)

    def test_wide_qrs(self) -> None:
        wide = self.assess(reading(qrs_duration_ms=130))
        very_wide = self.assess(reading(qrs_duration_ms=170))

        self.assertEqual((CAUTION, ("wide_qrs",)), (wide.status_level, wide.flags))
        self.assertEqual(CRITICAL, very_wide.status_level)

    def test_long_qtc(self) -> None:
        long = self.assess(reading(qtc_ms=480))
        very_long = self.assess(reading(qtc_ms=510))

        self.assertEqual((CAUTION, ("prolonged_qtc",)), (long.status_level, long.flags))
        self.assertEqual(CRITICAL, very_long.status_level)

    def test_the_worst_finding_decides_and_all_findings_are_flagged_in_rule_order(self) -> None:
        result = self.assess(reading(heart_rate=120, spo2=88, qtc_ms=480))

        self.assertEqual(CRITICAL, result.status_level)
        self.assertEqual(("tachycardia", "low_spo2", "prolonged_qtc"), result.flags)

    def test_a_caution_score_is_in_the_caution_band_and_a_critical_one_above_it(self) -> None:
        caution = self.assess(reading(heart_rate=120))
        critical = self.assess(reading(heart_rate=160))

        self.assertEqual(25, caution.risk_score)
        self.assertEqual(60, critical.risk_score)

    def test_the_score_never_exceeds_one_hundred(self) -> None:
        result = self.assess(reading(heart_rate=160, spo2=85, qtc_ms=520, qrs_duration_ms=170))

        self.assertEqual(100, result.risk_score)

    def test_age_and_cardiac_history_raise_the_score_but_never_the_status(self) -> None:
        context = PatientContext(age=80, prior_diagnosis=("Prior MI", "Hypertension"))

        result = self.assess(NORMAL, context)

        self.assertEqual(STABLE, result.status_level)
        self.assertEqual(10, result.risk_score)

    def test_history_words_match_whole_words_only(self) -> None:
        mild = self.assess(NORMAL, PatientContext(age=40, prior_diagnosis=("mild anxiety",)))
        heart_failure = self.assess(NORMAL, PatientContext(age=40, prior_diagnosis=("Heart Failure",)))

        self.assertEqual(0, mild.risk_score)
        self.assertEqual(5, heart_failure.risk_score)

    def test_a_value_next_to_a_threshold_lowers_the_confidence(self) -> None:
        borderline = self.assess(reading(heart_rate=111))
        clear = self.assess(reading(heart_rate=160))

        self.assertEqual(0.8, round(borderline.confidence_score, 2))
        self.assertEqual(0.95, clear.confidence_score)

    def test_confidence_stays_inside_zero_to_one(self) -> None:
        result = self.assess(reading(heart_rate=111, spo2=94, qtc_ms=461))

        self.assertGreaterEqual(result.confidence_score, 0.5)
        self.assertLessEqual(result.confidence_score, 1.0)


if __name__ == "__main__":
    unittest.main()
