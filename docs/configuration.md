# Configuration

`AddRailwayDashboard(name, browserToken, otlpApiKey)` requires distinct secret Aspire parameters. Configure `.PublishToRailway(target, (RailwayDashboardOptions options) => { ... })` with an explicitly typed callback when setting properties shared with the generic provider options.

| Option | Behaviour |
| --- | --- |
| ServiceName | Defaults to resource name; recorded identity protects against accidental renames. |
| OwnershipMode / ExistingServiceId | Shared provider creation/adoption policy; explicit ID is required for unmarked adoption. |
| Image | Tested immutable Aspire 13.6 Linux amd64 image digest; replacement must also be immutable. |
| Region | Shared provider Railway region identifier. |
| CustomDomain | Frontend HTTPS custom domain; operator configures DNS. |
| MemoryGB / VCpus | Optional provider resource limits. |
| DeploymentTimeoutSeconds | Positive timeout, default 600. |

Frontend listens on 18888 and receives the public HTTPS domain. Private OTLP gRPC 18889 and HTTP 18890 accept only the ingestion key. `WithRailwayDashboard` configures HTTP/protobuf OTLP with `x-otlp-api-key`, and orders workload deployment after dashboard readiness. Instrument workloads with OpenTelemetry exporters; environment configuration alone cannot instrument application code.
