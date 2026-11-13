# AI and risk service architecture

The FastAPI service in `back-end/back/cardiosense-ai` uses deterministic `rules-v0.1-prototype`. Its confidence is heuristic and its output is for research. No trained model is integrated into this service.

| Endpoint | Input | Output |
|---|---|---|
| POST /analyze | patientId, features {prIntervalMs, qrsDurationMs, qtQtcMs, heartRate, spo2}, patientContext {age, priorDiagnosis} | statusLevel, confidenceScore, modelVersion, riskScore, flags |
| POST /report | period, patientContext, statistics {averageHeartRate, highestRiskScore, alertsLoggedCount} | narrativeSummary |
| GET /health | None | status and modelVersion |

`app/main.py` owns route bindings. `schemas.py` validates and serializes JSON. `inference.py` supplies the classifier. `models/risk_classifier.py` owns thresholds and score composition. `report.py` converts period aggregates into narrative. Tests cover boundaries, reports and HTTP. The .NET `HttpRiskAssessmentService` uses a configurable timeout of 10 seconds by default. Readings are stored before inference. Patient reports omit the numeric score in both the service request and resulting text and response. Doctor output can contain clinical details.

## Future model gate

Define the intended outcome and reference annotation before collecting data. Document dataset rights and provenance. Partition by patient and time, compare with the rules baseline, report signal quality, sensitivity, specificity, false alarms and subgroup behavior. Track feature schema, model and dataset versions. Validate with the intended hardware and clinical reviewers before use. Protect the service with private networking and minimize diagnosis context and logs. See [audit](../audit-report/REPORT.md).
