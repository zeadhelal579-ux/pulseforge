# CardioSense Vercel frontend

This Vite app defaults to a browser-only demo with synthetic accounts and ECG readings. Demo data lives in the browser and can be reset from the interface.

## Run locally

Use Node 22. Run npm install, npm run dev, and open the URL printed by Vite. Use a demo account on the sign-in page. Run npm run build to check the deployable bundle.

## Deploy on Vercel

Select the front-deploy branch and use this app directory as the project root. The included vercel.json sets the build and SPA rewrite. The default demo needs no environment variables.

For a live backend, configure both VITE_API_BASE_URL and VITE_SIGNALR_HUB_URL. Set VITE_DEMO_MODE=false to require live mode. Configure the API's allowed origin and refresh-cookie policy for the deployed frontend, then redeploy.
