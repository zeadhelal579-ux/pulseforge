# PulseForge and CardioSense

PulseForge is a wearable cardiac monitoring research project. CardioSense is its web and backend platform.

| Branch | Workspace source | Contents |
|---|---|---|
| main | Workspace root | Project overview and documentation in Resource |
| back-end | back-end/back | ASP.NET Core API, SQL Server persistence, MQTT ingestion and FastAPI |
| front-end | front-end/CardioSense-Frontend-fixed | React client for the real backend |
| front-deploy | front Deploy/CardioSense-Frontend | Vercel-ready React demo and optional live API mode |

Start with the [documentation index](Resource/README.md), the [Arabic discovery brief](Resource/PulseForge-Discovery.pdf), or the [system design](Resource/system-design.md). The local audit used synthetic readings and SQL Server LocalDB. Device identity, broker TLS and hardware validation remain pilot gates.
