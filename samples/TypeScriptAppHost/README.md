# Minimal TypeScript AppHost Demo

This sample demonstrates the TypeScript AppHost shape for `PinguApps.Aspire.Hosting.Dashboard.Railway`.

Run from this directory:

```powershell
aspire restore --non-interactive
npm ci --no-audit --no-fund
npm run typecheck
aspire deploy --non-interactive --list-steps
```

For a live non-interactive deploy:

Create an environment-scoped project token under Railway project **Settings → Tokens**. The sample selects `ProjectToken` authentication and checks the configured project and environment against that token before changing a service.

```powershell
Set-Item Env:Parameters__railway-project-id $env:RAILWAY_PROJECT_ID
Set-Item Env:Parameters__railway-environment-id $env:RAILWAY_ENVIRONMENT_ID
Set-Item Env:Parameters__railway-token $env:RAILWAY_API_TOKEN
Set-Item Env:Parameters__site-key $env:RAILWAY_SITE_KEY
Set-Item Env:Parameters__dashboard-browser-token $env:DASHBOARD_BROWSER_TOKEN
Set-Item Env:Parameters__dashboard-otlp-key $env:DASHBOARD_OTLP_API_KEY
aspire deploy --non-interactive
```

`aspire.config.json` references the released packages. The worker image is a placeholder: replace it with your retained image digest before deployment.
