# PinguApps.Aspire.Hosting.Dashboard.Railway

Deploy an optional authenticated standalone Aspire dashboard to Railway through `aspire deploy`. Workloads send logs, traces and metrics over private Railway networking. The normal local Aspire dashboard remains unchanged.

```csharp
using Aspire.Hosting.Dashboard.Railway;
using Aspire.Hosting.Railway;

var web = builder.AddProject<Projects.Web>("web");
if (builder.ExecutionContext.IsPublishMode)
{
    var target = builder.AddRailwayTarget("railway",
        builder.AddParameter("railway-project-id", Current("Parameters__railway-project-id")),
        builder.AddParameter("railway-environment-id", Current("Parameters__railway-environment-id")),
        builder.AddParameter("railway-token", Current("Parameters__railway-token"), secret: true),
        builder.AddParameter("site-key", Current("Parameters__site-key")));
    var dashboard = builder.AddRailwayDashboard("diagnostics",
        builder.AddParameter("dashboard-browser-token", Current("Parameters__dashboard-browser-token"), secret: true),
        builder.AddParameter("dashboard-otlp-key", Current("Parameters__dashboard-otlp-key"), secret: true))
        .PublishToRailway(target);
    web.WithRailwayDashboard(dashboard)
        .PublishToRailway(target, options => options.Image = retainedImageDigest);
}

static string Current(string name) => Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"Missing deployment input: {name}");
```

Supply two separate randomly generated secrets, each at least 32 characters. The OTLP key uses URL-safe letters, digits, hyphens and underscores. Log in by entering the browser token into the UI; deployment outputs never include a token-bearing login URL.

This is a short-lived diagnostic viewer, not the durable shared OpenObserve service. It receives telemetry only: no production resource list, console-log stream, or remote AppHost controls. One replica, no persistent volume, no sleeping. Data is disposable across redeployments.

Target Aspire 13.6.0/.NET 10. Install `PinguApps.Aspire.Hosting.Dashboard.Railway`1.0.0 and the shared `PinguApps.Aspire.Hosting.Railway`1.0.0 provider. TypeScript AppHosts list both explicitly:

```json
"packages": {
  "PinguApps.Aspire.Hosting.Railway": "1.0.0",
  "PinguApps.Aspire.Hosting.Dashboard.Railway": "1.0.0"
}
```

See [install](docs/install.md), [configuration](docs/configuration.md), [deployment behaviour](docs/deployment-behaviour.md), [TypeScript](docs/getting-started-typescript.md), [security and outputs](docs/outputs-and-security.md), and [samples](docs/samples-and-demos.md).
