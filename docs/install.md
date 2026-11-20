# Install

Use .NET 10 and Aspire 17.0.0. Add `PinguApps.Aspire.Hosting.Dashboard.Railway` 1.0.5 to the AppHost using normal NuGet/central package management. It depends on `PinguApps.Aspire.Hosting.Railway` 1.1.5.

For TypeScript, set `sdk.version` to 17.0.0 and list both package IDs explicitly. Transitive NuGet references do not generate core target exports automatically. Run `aspire restore`, `npm ci`, then `npm run typecheck`.

CI and release validation restore the shared provider from NuGet.org. For an unpublished core change, run `eng/Prepare-RailwayDependency.ps1` and pass its feed explicitly to the package gate.
