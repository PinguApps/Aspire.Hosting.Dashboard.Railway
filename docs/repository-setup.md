# Repository setup

Before release publication:

1. Enable the Blacksmith GitHub App/runner for this repository (`blacksmith-2vcpu-ubuntu-2404`), matching the reference extensions.
2. Configure the `NUGET_USER` GitHub Actions secret with the NuGet.org publishing account name.
3. Configure a NuGet.org trusted publisher for owner `PinguApps`, repository `Aspire.Hosting.Dashboard.Railway`, workflow `publish.yml`. Publishing uses `NuGet/login` OIDC; no long-lived NuGet API key is referenced.
4. Publish `PinguApps.Aspire.Hosting.Railway` 1.1.5 before this dependent package. The initial publish is performed by the repository owner.

CI and release validation restore the published core dependency from NuGet.org. The release workflow pushes the dashboard package.

Normal CI needs no Railway credentials. To run the optional read-only live security test, set repository variable `RAILWAY_DASHBOARD_TEST_URL` to an existing test dashboard and dispatch PR validation with `run_live_railway=true`. Provisioning tests require a scoped project token and runtime dashboard secrets supplied outside source control.
