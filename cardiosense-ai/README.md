# CardioSense AI service (research prototype)

Internal FastAPI service the .NET backend calls to turn one set of ECG/vitals features into a status
(`Stable` / `Caution` / `Critical`) and to write the narrative of the "AI-Assisted Insights (Prototype)" card.

## Honest status

This is **not** a trained or clinically validated model. `app/models/risk_classifier.py` is a transparent
**rule-based prototype**: one threshold per measurement, all listed in `RULES` so a clinician can read and change
them. The thresholds are common adult reference limits and are an **assumption that needs clinical sign-off**.
`confidenceScore` is a heuristic ("is any value right next to a threshold?"), not a statistical probability.
A trained model (see `training/`) replaces the classifier later behind `app/inference.py`; nothing else changes.

## Contract (what `HttpRiskAssessmentService` in the backend sends and expects)

`POST /analyze`

```json
{ "patientId": "guid",
  "features": { "prIntervalMs": 152, "qrsDurationMs": 88, "qtQtcMs": [396, 418], "heartRate": 74, "spo2": 98 },
  "patientContext": { "age": 58, "priorDiagnosis": ["Prior MI"] } }
```
answers
```json
{ "statusLevel": "Stable", "confidenceScore": 0.95, "modelVersion": "rules-v0.1-prototype",
  "riskScore": 5, "flags": [] }
```

`POST /report`

```json
{ "period": "daily",
  "patientContext": { "age": 58, "priorDiagnosis": ["Prior MI"] },
  "statistics": { "averageHeartRate": 74.2, "highestRiskScore": 31, "alertsLoggedCount": 2 } }
```
answers `{ "narrativeSummary": "..." }`. The narrative only restates the supplied numbers.

`GET /health` answers `{ "status": "ok", "modelVersion": "..." }`.

Flags the prototype can raise: `tachycardia`, `bradycardia`, `low_spo2`, `prolonged_pr`, `short_pr`, `wide_qrs`,
`prolonged_qtc`. Flags are doctor-only; the backend never shows them to a patient.

## Rules (prototype thresholds)

| Flag | Caution | Critical |
|---|---|---|
| tachycardia (heart rate above) | 110 bpm | 150 bpm |
| bradycardia (heart rate below) | 50 bpm | 40 bpm |
| low_spo2 (below) | 94 % | 90 % |
| prolonged_pr (above) | 200 ms | 300 ms |
| short_pr (below) | 120 ms | - |
| wide_qrs (above) | 120 ms | 160 ms |
| prolonged_qtc (above) | 460 ms | 500 ms |

Score: caution finding = 25 points, critical finding = 60, plus up to 10 for age >= 65 and known cardiac history
(context never changes the status by itself), capped at 100.

## Run and test

```bash
pip install -r requirements-dev.txt
uvicorn app.main:app --port 8000            # http://localhost:8000/docs
python -m unittest discover -s tests -t .   # the API tests are skipped if fastapi/httpx are missing
```

## Security

The service has no authentication: it must never be exposed to the public internet. Reach it only from the backend's
private network (docker-compose publishes no port for it).
