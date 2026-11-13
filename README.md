# CardioSense frontend

This React and Vite client connects to the real ASP.NET Core API and SignalR hub.

1. Install dependencies with npm ci.
2. Start the backend locally. The Vite proxy forwards /api and /hubs to https://localhost:5001 by default.
3. Run npm run dev and open http://localhost:5173.

To connect to a different environment, use .env.example to set VITE_API_BASE_URL and VITE_SIGNALR_HUB_URL. Vite variables are public.

Run npm run build, npm run lint and npm test before publishing changes. Use synthetic accounts and device readings for local testing.
