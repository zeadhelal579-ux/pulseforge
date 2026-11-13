"""
File           : test_report.py
Layer          : AI service / tests
Purpose        : Verifies the report narrative only restates the numbers it is given.
Logic          : The narrative must never invent a finding. These tests check that every sentence is driven by an
                 input, that missing numbers are simply left out, and that the prototype disclaimer is always there.
Implementation : Standard-library unittest.
"""
import unittest

from app.models.risk_classifier import PatientContext
from app.report import DISCLAIMER, ReportStatistics, build_narrative

CONTEXT = PatientContext(age=58, prior_diagnosis=("Prior MI",))


class ReportTests(unittest.TestCase):
    """Tests for build_narrative."""

    def test_the_narrative_restates_the_given_numbers(self) -> None:
        text = build_narrative("daily", CONTEXT, ReportStatistics(74.2, 31, 2))

        self.assertIn("the last 24 hours", text)
        self.assertIn("Patient age 58; recorded history: Prior MI.", text)
        self.assertIn("average heart rate was 74.2 bpm", text)
        self.assertIn("within the typical adult resting range", text)
        self.assertIn("highest risk score reached was 31 out of 100", text)
        self.assertIn("2 alerts were logged.", text)

    def test_the_period_selects_the_wording(self) -> None:
        stats = ReportStatistics(None, None, 0)

        self.assertIn("the last 7 days", build_narrative("weekly", CONTEXT, stats))
        self.assertIn("the last 30 days", build_narrative("monthly", CONTEXT, stats))

    def test_missing_numbers_are_left_out_not_invented(self) -> None:
        text = build_narrative("daily", PatientContext(age=40), ReportStatistics(None, None, 0))

        self.assertNotIn("heart rate", text)
        self.assertNotIn("risk score", text)
        self.assertIn("No alerts were logged.", text)

    def test_a_heart_rate_outside_the_typical_range_is_described_as_such(self) -> None:
        high = build_narrative("daily", CONTEXT, ReportStatistics(112.0, None, 0))
        low = build_narrative("daily", CONTEXT, ReportStatistics(52.5, None, 0))

        self.assertIn("above the typical adult resting range", high)
        self.assertIn("below the typical adult resting range", low)

    def test_one_alert_is_singular(self) -> None:
        text = build_narrative("daily", CONTEXT, ReportStatistics(None, None, 1))

        self.assertIn("1 alert was logged.", text)

    def test_the_prototype_disclaimer_is_always_present(self) -> None:
        for stats in (ReportStatistics(None, None, 0), ReportStatistics(80.0, 10, 3)):
            with self.subTest(stats=stats):
                self.assertTrue(build_narrative("daily", CONTEXT, stats).endswith(DISCLAIMER))


if __name__ == "__main__":
    unittest.main()
