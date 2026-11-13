# PulseForge and CardioSense system design

**Status:** Local research prototype verified with synthetic inputs. Pilot design pending review. **Scope:** Wearable concept, web client, telemetry and risk service.

## 1. Abstract

PulseForge studies whether patient-specific electrode placement can support more informative continuous ECG monitoring. CardioSense receives readings, stores vitals and ECG features, computes a research risk state and displays the authorized view for each role.

## 2. Goals and non-goals

| Goal | Deferred scope |
|---|---|
| Show synthetic ECG, heart rate and oxygen saturation in the browser | Clinical diagnostic claims or automated treatment decisions |
| Limit access to the patient and assigned doctors | Hospital EHR integration and billing |
| Retain readings during an inference outage | Trained ML, production BLE gateway and camera verification |
| Expose stale readings and document recovery | Production availability targets pending workload and clinical ownership |

## 3. Background and problem statement

A clinic snapshot covers a limited time window. PulseForge proposes two fixed reference electrodes and two clinician-positioned electrodes to investigate the value of targeted monitoring between visits. The main uncertainties are signal quality, electrode geometry and whether additional information changes clinician decisions. Earlier sources describe differing fixed positions. Clavicular placement is a documented design candidate, pending bench validation.

## 4. Proposed architecture

[System diagram and contract](architecture-overview.md).

| Component | Responsibility | Failure behavior |
|---|---|---|
| Wearable or fixture | Send ECG features, optional samples and vitals | Mark the stream stale when samples stop |
| MQTT broker | Route device messages to ingestion | Worker reconnects. Clean sessions leave a delivery gap |
| ASP.NET Core and SQL Server | Authorize, persist, assemble REST responses and push events | Restore history from SQL after reconnect |
| FastAPI | Compute rule-based status and narrative | Preserve readings and expose lack of a new assessment |
| React | Render role-specific dashboards and live stream | Show disconnected, loading and empty states |

## 5. Request lifecycle

Device publishes a reading to its serial topic. The worker validates the payload and pairing, writes vitals and ECG features, then broadcasts samples and vitals. The worker calls FastAPI using the configured HTTP timeout. The backend persists the returned assessment and any alert, then sends role-specific events. The browser reads stored history via REST.

## 6. API and data contracts

[Shared contract](architecture-overview.md) defines API IDs, event names, timestamps, units and ownership. The active wire schema is derived from the endpoint and DTO sources. Keep firmware, service and web changes compatible with those contracts.

## 7. Consistency idempotency and replay

MQTT QoS 1 permits duplicates. Current writes and live broadcasts are best effort across stages. Before a pilot, define device message identity, impose a unique storage key, test duplicate and out-of-order delivery, and decide how delayed messages affect current status. Record processing failures by stage. The Critical alert remains open until authorized resolution.

## 8. Security and privacy considerations

Enforce ownership in the API and SignalR. Use independent credentials and broker ACLs per device before deployment. Rotate secrets, restrict SQL and FastAPI to private networking, and redact token-bearing URLs from logs. Define doctor verification, patient consent for assignment, clinical review ownership and retention before handling real data.

## 9. Operational readiness

Synthetic E2E, 26 SQL-backed API integration tests and an initial EF migration ran locally. Compose deployment, production TLS, device isolation, backup restoration and real hardware remain pilot gates. Track accepted telemetry, end-to-end delay, last reading age, inference errors and recovery time. Select SLOs only after workload measurement.

## 10. Alternatives considered

| Alternative | Decision |
|---|---|
| Polling for waveform samples | REST for history and SignalR for streaming |
| Dedicated time-series database | Measure SQL Server before adding a datastore |
| Redis in the first deployment | Compose profile remains optional pending scale-out |
| Trained model from day one | Use versioned rules until outcome, dataset and evaluation protocol are approved |

## 11. Open questions

Electrode geometry, number of synchronized channels, device credential issuer, ordering and retention windows, doctor and patient consent workflow, alert ownership, and deployment SLOs.

## 12. Decision and next steps

Keep the synthetic demonstration reproducible. Complete broker isolation and fault recovery. Validate the wearable on a bench before any human pilot. Use the [blueprint milestones](blueprint.md) for entry and exit criteria.
