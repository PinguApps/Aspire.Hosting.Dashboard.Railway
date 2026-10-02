# Install

Use .NET 10 and Aspire 13.6.0. Add `PinguApps.Aspire.Hosting.Dashboard.Railway`1.0.0 to the AppHost using normal NuGet/central package management. It depends on `PinguApps.Aspire.Hosting.Railway`1.0.0.

For TypeScript, set `sdk.version` to 13.6.0 and list both package IDs explicitly. Transitive NuGet references do not generate core target exports automatically. Run `aspire restore`, `npm ci`, then `npm run typecheck`.

Before initial publication, CI builds the shared provider from the immutable source commit in `eng/Prepare-RailwayDependency.ps1`. Publish the shared provider first, then this package. No binaries are committed.
