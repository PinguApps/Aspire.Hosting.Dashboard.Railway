# Samples and verification

- [C# sample](../samples/AppHostSnippets/RailwayDashboardAppHostSnippets.cs) compiles in the contract test project.
- [TypeScript sample](../samples/TypeScriptAppHost/apphost.mts) mirrors the packed-package fixture.
- Run `dotnet test Aspire.Hosting.Dashboard.Railway.slnx -c Release`.
- Run `pwsh ./eng/Validate-TypeScriptAppHostPackage.ps1` for the real packed-NuGet TypeScript contract.

The live PIN-646 test consumer runs outside this repository and leaves its Railway dashboard and private .NET telemetry producer deployed for inspection. Credentials remain outside the repository. See `docs/live-validation.md` for the recorded results.
