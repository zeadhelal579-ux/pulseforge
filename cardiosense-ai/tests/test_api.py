"""
File           : test_api.py
Layer          : AI service / tests
Purpose        : Verifies the HTTP contract the .NET backend depends on (camelCase JSON, required fields, status codes).
Logic          : The backend's HttpRiskAssessmentService rejects any answer without statusLevel, confidenceScore and
                 modelVersion, and expects exactly the field names in AiContracts.cs. These tests send the same JSON the
                 backend sends and check the same fields come back.
Implementation : FastAPI's TestClient (needs the packages in requirements-dev.txt). They are skipped, not failed,
                 when FastAPI is not installed, so the pure-Python tests still run anywhere.
"""
import unittest

try:
    from fastapi.testclient import TestClient

    from app.main import app
except ModuleNotFoundError:  # FastAPI / httpx not installed in this environment
    TestClient = None
    app = None

PATIENT_ID = "3f2b8c1e-0000-4000-8000-000000000001"

ANALYZE_BODY = {
    "patientId": PATIENT_ID,
    "features": {"prIntervalMs": 152, "qrsDurationMs": 88, "qtQtcMs": [396, 418], "heartRate": 74, "spo2": 98},
    "patientContext": {"age": 58, "priorDiagnosis": ["Prior MI", "Hypertension"]},
}

REPORT_BODY = {
    "period": "daily",
    "patientContext": {"age": 58, "priorDiagnosis": ["Prior MI"]},
    "statistics": {"averageHeartRate": 74.2, "highestRiskScore": 31, "alertsLoggedCount": 2},
}


@unittest.skipIf(TestClient is None, "fastapi / httpx are not installed")
class ApiContractTests(unittest.TestCase):
    """Tests for /health, /analyze and /report."""

    def setUp(self) -> None:
        self.client = TestClient(app)

    def test_health_reports_ok_and_the_model_version(self) -> None:
        body = self.client.get("/health").json()

        self.assertEqual("ok", body["status"])
        self.assertTrue(body["modelVersion"].startswith("rules-"))

    def test_analyze_returns_every_field_the_backend_requires(self) -> None:
        response = self.client.post("/analyze", json=ANALYZE_BODY)

        self.assertEqual(200, response.status_code)
        body = response.json()
        self.assertEqual({"statusLevel", "confidenceScore", "modelVersion", "riskScore", "flags"}, set(body))
        self.assertEqual("Stable", body["statusLevel"])
        self.assertEqual([], body["flags"])

    def test_analyze_flags_a_critical_reading(self) -> None:
        body = {**ANALYZE_BODY, "features": {**ANALYZE_BODY["features"], "heartRate": 170, "spo2": 85}}

        answer = self.client.post("/analyze", json=body).json()

        self.assertEqual("Critical", answer["statusLevel"])
        self.assertEqual(["tachycardia", "low_spo2"], answer["flags"])

    def test_analyze_rejects_a_qt_pair_of_the_wrong_length(self) -> None:
        body = {**ANALYZE_BODY, "features": {**ANALYZE_BODY["features"], "qtQtcMs": [396]}}

        self.assertEqual(422, self.client.post("/analyze", json=body).status_code)

    def test_analyze_rejects_a_missing_feature(self) -> None:
        features = {key: value for key, value in ANALYZE_BODY["features"].items() if key != "spo2"}

        response = self.client.post("/analyze", json={**ANALYZE_BODY, "features": features})

        self.assertEqual(422, response.status_code)

    def test_report_returns_the_narrative(self) -> None:
        response = self.client.post("/report", json=REPORT_BODY)

        self.assertEqual(200, response.status_code)
        self.assertIn("average heart rate was 74.2 bpm", response.json()["narrativeSummary"])

    def test_report_accepts_null_statistics(self) -> None:
        body = {**REPORT_BODY, "statistics": {"averageHeartRate": None, "highestRiskScore": None, "alertsLoggedCount": 0}}

        self.assertEqual(200, self.client.post("/report", json=body).status_code)

    def test_report_rejects_an_unknown_period(self) -> None:
        self.assertEqual(422, self.client.post("/report", json={**REPORT_BODY, "period": "yearly"}).status_code)


if __name__ == "__main__":
    unittest.main()
