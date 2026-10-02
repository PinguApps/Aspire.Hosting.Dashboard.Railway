# TypeScript AppHost

See [apphost.mts](../samples/TypeScriptAppHost/apphost.mts). `addRailwayDashboard` accepts secret parameter handles. `dashboard.publishToRailway(target, options)` uses the callback-free DTO. `container.withRailwayDashboard(dashboard)` and `project.withRailwayDashboard(dashboard)` configure private telemetry in publish mode.

Declare both package IDs explicitly in `aspire.config.json`. Restore generated exports with Aspire 13.6.0, then run the sample's typecheck. Parameter fixture values are placeholders; supply actual production secrets outside source control.

`eng/Validate-TypeScriptAppHostPackage.ps1` packs the NuGet, restores an isolated cache, generates the guest API, typechecks the complete consumer and inspects publish/deploy steps. It fails on every native command error.

Keep local workloads outside `if (await builder.executionContext().isPublishMode())`. Create Railway target parameters, dashboard secrets and publishing calls inside that guard. Supply current private inputs with `builder.addParameter(name, { value: currentValue, secret: true })`; non-secret IDs/site keys omit `secret`. The sample reads the environment and leaves fixture defaults as placeholders. Explicit values override cached deployment parameters, which matters when rotating tokens; never set `publishValueAsDefault` for secrets.
