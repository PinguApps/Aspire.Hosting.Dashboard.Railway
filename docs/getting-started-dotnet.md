# C# AppHost

The [compile-validated sample](../samples/AppHostSnippets/RailwayDashboardAppHostSnippets.cs) declares target, secret parameters, dashboard, and a retained-image worker.

Use `.WithRailwayDashboard(dashboard)` before the workload's `.PublishToRailway(target)` so the shared provider discovers the deployment dependency. The workload must have a retained image digest and its own OpenTelemetry logs, metrics and tracing instrumentation/exporters.

Generate browser and ingestion secrets independently with a cryptographic RNG. Keep the Railway control-plane token separate from both runtime secrets. Set parameter values through user secrets, environment variables or deployment configuration outside the repository.

Declare local workloads first. Create Railway target parameters, dashboard secrets and publishing configuration only inside `if (builder.ExecutionContext.IsPublishMode)` (or return early from a deployment helper in run mode), so local runs need no deployment credentials. Pass each current private value explicitly using `AddParameter(name, currentValue, secret: true)` for secrets; the sample reads environment values without exposing them.
