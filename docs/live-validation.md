# Live validation

Validated with packed NuGet consumers outside this repository on 2026-10-02. Resources remain running in the dedicated **PIN-646 Dashboard Integration** project for inspection.

| Resource | Identity / URL |
| --- | --- |
| Project | `5d84f267-21ab-47b7-bd1d-1234a812a369` |
| Production environment | `c713bde5-e7eb-4723-815c-ac0063241a1b` |
| Dashboard service | `87e4b135-b306-4831-a73c-8e0139eba542` |
| Dashboard frontend | https://pin646-diagnostics-production.up.railway.app |
| .NET test probe | https://pin646-telemetry-probe-production.up.railway.app |

Verified:

- `aspire deploy` provisioned the dashboard and dependent probe; Railway reported exact deployment `SUCCESS`.
- Immutable dashboard image, explicit service name, region, memory/vCPU limits, timeout, public frontend target 18888 and private ingestion 18889/18890 were configured.
- Anonymous frontend access redirects to `/login`. Telemetry HTTP API returns 404. A public request to OTLP returns 401; dashboard logs confirm OTLP connection types are disabled on the frontend listener.
- BrowserToken API accepts the current secret, sets its authentication cookie, and allows `/structuredlogs` 200. An obsolete token is rejected.
- A .NET OpenTelemetry producer uses package-provided private HTTP/protobuf endpoint and header configuration. Its private invalid-key test returns 401.
- Unchanged repeated deployment reused both services, domains and exact successful deployment IDs, with no image rebuild/push or redeployment.
- Wrong site marker and configured service-name drift fail the shared target preflight before any apply step.
- Runtime secrets and token-bearing login URLs are absent from provider outputs/startup logs.

The probe is a test utility: an immutable official .NET SDK container compiles small external test source at startup because the available GitHub credential cannot push GHCR packages. Production examples use release images already retained by PIN-647. All provisioning still uses `aspire deploy` and the shared publisher.

Tokens and temporary consumer files remain outside the repository. Custom-domain DNS is an operator responsibility; the live test uses Railway's generated HTTPS domain. Shared provider contract tests cover domain and ownership permutations.

Authenticated external Chrome verified all three telemetry types: Structured logs with PIN646_DASHBOARD_LOG_MARKER, child trace pin646-dashboard-live-trace, and the pin646.dashboard.probe.requests metric graph. The normal local Aspire dashboard started successfully while the extra resource remained NotStarted; no duplicate dashboard container ran.

A second external packed-NuGet TypeScript AppHost completed actual aspire deploy successfully against the same existing service; its callback-free DTO preserved image defaults and private secret references, reused the current deployment, and exported the token-free frontend URL. Supply TypeScript runtime parameter values through Parameters__ environment variables; the fixture's Parameters object is only placeholder documentation.

During browser verification, automation exposed the initial frontend token in a diagnostic attribute read. That frontend credential was replaced before handoff; the new token was verified to work and the old token was verified to fail. No credential value is included in this repository or the evidence summary. The private OTLP key and Railway project token were unaffected.
