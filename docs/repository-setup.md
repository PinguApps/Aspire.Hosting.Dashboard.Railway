# Repository setup

Before release publication:

1. Enable the Blacksmith GitHub App/runner for this repository (`blacksmith-2vcpu-ubuntu-2404`), matching the reference extensions.
2. Configure the `NUGET_USER` GitHub Actions secret with the NuGet.org publishing account name.
3. Configure a NuGet.org trusted publisher for owner `PinguApps`, repository `Aspire.Hosting.Dashboard.Railway`, workflow `publish.yml`. Publishing uses `NuGet/login` OIDC; no long-lived NuGet API key is referenced.
4. Publish `PinguApps.Aspire.Hosting.Railway` 1.0.0 before this dependent package. The initial publish is performed by the repository owner.

CI builds its core dependency from the immutable source commit pinned in `eng/Prepare-RailwayDependency.ps1`; it does not publish that dependency. Only this repository's package filename is pushed by `publish.yml`.

Normal CI needs no Railway credentials. To run the optional read-only live security test, set repository variable `RAILWAY_DASHBOARD_TEST_URL` to an existing test dashboard and dispatch PR validation with `run_live_railway=true`. Provisioning tests require a scoped project token and runtime dashboard secrets supplied outside source control.
