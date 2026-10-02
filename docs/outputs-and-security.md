# Outputs and security

Use the shared provider's `dashboard.GetRailwayOutputs()` for service ID, deployment ID, private hostname and token-free public HTTPS URL. Credentials remain secret parameter references and are not exported as URLs.

C# and TypeScript can also use `dashboard.GetRailwayDashboardUrl()` / `dashboard.getRailwayDashboardUrl()` for the deferred frontend URL after marking the dashboard for Railway publishing. It contains no login token and returns an empty expression in local run mode, where the extra service is stopped.

BrowserToken protects frontend access. A separate API key protects both private OTLP protocols. Anonymous mode is false. The optional telemetry HTTP API is disabled; Aspire 13.6's MCP telemetry access consequently has no exposed API. Startup INFO messages from `Aspire.Dashboard.DashboardWebApplication` are suppressed because the stock image prints a token-bearing login URL.

Secrets must differ and contain at least 32 characters. Generate 128 bits or more entropy for each; length validation cannot establish entropy. OTLP keys must use URL-safe letters, digits, hyphens and underscores to remain valid exporter header values.

Telemetry itself can contain sensitive data. Enter the browser token into the login form, rotate it when sharing access, and instrument applications so secrets are not logged. No public OTLP domain/TCP proxy is provisioned.

Settings follow the official [dashboard configuration](https://aspire.dev/dashboard/configuration/) and [security guidance](https://aspire.dev/dashboard/security-considerations/), verified against the pinned 13.6 container.

For rotation, pass the current secret explicitly: `builder.AddParameter("dashboard-browser-token", currentBrowserToken, secret: true)` and `builder.AddParameter("dashboard-otlp-key", currentOtlpKey, secret: true)`. Read those values from private environment/configuration outside source control. The explicit current-value overload overrides Aspire's same-name cached deployment parameter; changing configuration alone can retain its old value. TypeScript uses `builder.addParameter(name, { value: currentSecret, secret: true })`. Never use `publishValueAsDefault` for secrets. Redeploy, verify the new browser token/private OTLP key, and confirm the old credential fails.
