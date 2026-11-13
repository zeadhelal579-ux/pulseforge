# Embedded device and IoT architecture

The wearable concept uses two fixed electrodes for baseline rhythm and two clinician-positioned electrodes for localized monitoring. AD8232 and MAX30102 are proposed acquisition components and ESP32 is the embedded controller. Simultaneous channel count, reference method, calibration and final electrode geometry require bench validation. Graphene–PEDOT and the Modified Central Reference are research proposals.

| MQTT topic | Message fields | Backend behavior |
|---|---|---|
| devices/{serial}/telemetry | timestamp optional, heartRate, spo2, prIntervalMs, qrsDurationMs, qtMs, qtcMs, optional ecgSamples and features | Resolve serial from topic, validate pairing, persist measurements and features, broadcast and assess |
| devices/{serial}/status | batteryLevel, connectionMode WiFi or Bluetooth, optional firmwareVersion and signalStrengthDbm | Update device state and last seen |

The local MQTT 3.1.1 path was tested with a synthetic publisher. WiFi direct and BLE through a phone gateway are target transport designs. Define a gateway identity handoff that preserves the device ACL. For deployment, use TLS and unique credentials restricted to the device's exact write topics. Give the ingestion identity only required subscriptions. Design credential enrollment, revocation, rollover and offline limits. See [Mosquitto Dynamic Security](https://mosquitto.org/documentation/dynamic-security/).

| Proposed module | Contract and verification |
|---|---|
| Acquisition | Timestamped calibrated windows and quality metadata. Measure sample jitter and contact quality |
| Feature extraction | Derive interval fields from defined lead and sample rate. Compare with reference fixtures |
| Encoder | Validate units, payload size, schema and message identity against .NET parser |
| Transport | Deliver over WiFi or approved BLE gateway. Bound buffer and test reconnect and overflow |
| Provisioning | Bind serial to credential and support revocation. Verify allowed topic scope |
| Bench harness | Replay known signals, measure motion artifacts and timing, document the reference configuration |

QoS 1 may redeliver. Current clean session can lose messages during downtime. Pilot work must set limits for age, clock skew, duplicates and offline retention. The camera path, signed firmware update design and BLE app remain open. See [flows](flows.md).
