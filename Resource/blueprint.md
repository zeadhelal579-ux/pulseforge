# Implementation blueprint

| Page | Route | Role | Outcome |
|---|---|---|---|
| P01 | /auth | Visitor | Sign in or create an account |
| P02 | /patient | Patient | See own ECG, vitals, status, alerts and report |
| P03 | /doctor/patients | Doctor | Search, filter and assign patients |
| P04 | /doctor/patients/:patientId | Assigned doctor | Inspect clinical details and log actions |
| P05 | /settings | Authenticated | Set language and pair a patient device |

| Capability | Acceptance evidence |
|---|---|
| F01 Session | Login, reload and logout behave correctly. Unauthorized navigation is redirected |
| F02 Ownership | Patient accesses own file, assigned doctor accesses that patient, paired device belongs to one patient |
| F03 Telemetry | Synthetic ECG and vitals appear live and vitals are retrievable after reconnect |
| F04 Alerts | Status reaches both roles. Clinical score and flags reach only the doctor |
| F05 Review | Period report uses stored aggregates and doctor actions are scoped to the patient |

## Milestones

1. **Local integration.** Start SQL, FastAPI, broker, API and web. Reproduce patient and doctor browser tests. See the [audit](../audit-report/REPORT.md).
2. **Engineering pilot readiness.** Provision unique device identities, isolate topics, define message IDs, test outage and restore, and agree on data retention.
3. **Bench validation.** Document electrode layout and channel acquisition. Compare captured signal quality against a pre-agreed reference and measure loss, drift and motion artifacts.
4. **Limited pilot.** Start only after clinical, privacy and operational review. Verify escalation and rollback with named owners.

**Blueprint deltas from earlier proposals:** SQL holds time-series data in the prototype. Redis remains optional. FastAPI runs a rules classifier. BLE gateway, camera verification and trained ML remain proposed. The Streamlit demo mentioned in the presentation has no documented runtime integration with the .NET service.
