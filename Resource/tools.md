# Project tools

| Layer | Tools | Rationale |
|---|---|---|
| Web | React 19, TypeScript, Vite 6, Tailwind 4, React Router, TanStack Query, Zustand, Axios, i18next, React Hook Form, Zod, Recharts | Authenticated SPA, cached REST, memory sessions, RTL and dashboard rendering |
| Backend | .NET 10, ASP.NET Core Minimal APIs, Mediator, FluentValidation, Identity, SignalR, MQTTnet | Separate use cases, HTTP, real-time monitoring and device transport |
| Storage | SQL Server, EF Core, LocalDB for Windows testing | Durable relational ownership and time-indexed history |
| Inference | Python 3.11 in the audit, FastAPI, Pydantic, Uvicorn | Small versioned service and typed contracts |
| Device | ESP32, AD8232 and MAX30102 in the concept, ESP-IDF candidate, Mosquitto and MQTT 3.1.1 | Embedded acquisition and a transport boundary for the backend |
| Verification | ESLint, Vitest, xUnit, Python unittest, Playwright, axe, package audits | Build, rules, access, real SQL and browser checks |
| Deployment | Docker Compose for development, Vite proxy locally, Redis optional profile, independent Vercel snapshot | Separate runtime environments |

Version authorities are [npm lockfile](https://github.com/zeadhelal579-ux/pulseforge/blob/front-end/package-lock.json), [NuGet versions](https://github.com/zeadhelal579-ux/pulseforge/blob/back-end/cardiosense-backend/Directory.Packages.props) and [Python requirements](https://github.com/zeadhelal579-ux/pulseforge/blob/back-end/cardiosense-ai/requirements.txt). Pin the resolved Python image before release. Public Vite settings are embedded in the client bundle. See [Vite environment docs](https://vite.dev/guide/env-and-mode.html) and [ESP-IDF documentation](https://docs.espressif.com/projects/esp-idf/en/stable/esp32/get-started/index.html).

## Verification commands

- Web root: `npm ci`, `npm run build`, `npm run lint`, `npm test`, `npm audit`.
- .NET root: `dotnet build CardioSense.sln -c Release`, `dotnet test CardioSense.sln -c Release`.
- AI root: `python -m unittest discover -s tests -t .`.
- [Local runtime and fixtures](../audit-report/01-environment.md) lists database and browser setup.
