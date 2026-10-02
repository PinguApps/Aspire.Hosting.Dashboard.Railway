# Overview

The package adds a standalone diagnostic dashboard, its hardened container settings, and private authenticated OTLP references for deployed .NET projects or containers. Railway provisioning, ownership, idempotence, image selection and deployment state belong to the shared Railway provider.

The dashboard is optional. Omit `AddRailwayDashboard` and `WithRailwayDashboard` when production diagnostics are disabled. OpenObserve is operated separately and is not provisioned here.

The ordinary local Aspire dashboard remains enabled. The standalone resource stays explicitly stopped locally, and `WithRailwayDashboard` leaves local telemetry settings alone.
