# CardioSense backend

The backend contains the ASP.NET Core API and worker in cardiosense-backend and the private FastAPI service in cardiosense-ai. SQL Server stores accounts and readings. MQTT receives device messages. SignalR pushes authorized live updates.

## Run locally

1. Open cardiosense-backend and copy .env.example to .env. Fill in the development-only values.
2. Run docker compose up --build. The development API listens on localhost:5000. FastAPI stays inside the Compose network.
3. Use GET /health/live and GET /health/ready to check the API and database.

On Windows, the API also supports SQL Server LocalDB in Development. Supply a signing key through User Secrets or an environment variable and run dotnet run --project src/CardioSense.Api from cardiosense-backend.

## Build and test

- Run dotnet build CardioSense.sln -c Release and dotnet test CardioSense.sln -c Release from cardiosense-backend. Integration tests use Docker by default. CARDIOSENSE_TEST_SQL can point to a dedicated empty SQL test database.
- From cardiosense-ai, install requirements-dev.txt and run python -m unittest discover -s tests -t .
- Apply the initial EF migration to a new database before using migrations in deployment. Existing EnsureCreated databases need a reviewed baseline or migration plan.

The risk service uses versioned research rules. The development broker uses a shared credential. Before a pilot, configure a separate identity and topic ACL for each device.
