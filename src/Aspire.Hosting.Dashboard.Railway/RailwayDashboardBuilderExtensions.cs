#pragma warning disable CA1708 // The analyzer treats distinct C# extension blocks as case-only duplicate members.

using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Railway;

namespace Aspire.Hosting.Dashboard.Railway;

/// <summary>Publishes a standalone diagnostic dashboard without changing the local Aspire dashboard.</summary>
public static class RailwayDashboardBuilderExtensions
{
    /// <summary>Declares a publish-only dashboard from an AppHost.</summary>
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>Adds a standalone dashboard which stays explicitly stopped in local run mode.</summary>
        [AspireExport("pinguapps.railway.dashboard.add", MethodName = "addRailwayDashboard")]
        public IResourceBuilder<RailwayDashboardResource> AddRailwayDashboard(string name,
            IResourceBuilder<ParameterResource> browserToken, IResourceBuilder<ParameterResource> otlpApiKey)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(browserToken);
            ArgumentNullException.ThrowIfNull(otlpApiKey);
            if (!browserToken.Resource.Secret || !otlpApiKey.Resource.Secret || browserToken.Resource == otlpApiKey.Resource)
            {
                throw new ArgumentException("Dashboard frontend and OTLP credentials require distinct secret parameters.");
            }

            return builder.AddResource(new RailwayDashboardResource(name, browserToken.Resource, otlpApiKey.Resource))
                .WithImage("mcr.microsoft.com/dotnet/aspire-dashboard")
                .WithImageSHA256("94f41463f93ff8f87257a9ae8681a190fcd73e8e10cb46884160a6a6a8321356")
                .WithExplicitStart().ExcludeFromManifest();
        }
    }

    /// <summary>Configures dashboard publishing.</summary>
    extension(IResourceBuilder<RailwayDashboardResource> builder)
    {

        /// <summary>Publishes authenticated frontend HTTPS and private authenticated OTLP listeners.</summary>
        [AspireExportIgnore(Reason = "TypeScript uses the callback-free DTO overload.")]
        public IResourceBuilder<RailwayDashboardResource> PublishToRailway(
            IResourceBuilder<RailwayTargetResource> target, Action<RailwayDashboardOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(target);
            RailwayDashboardOptions options = new();
            configure?.Invoke(options);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.DeploymentTimeoutSeconds);
            if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
            {
                return builder;
            }

            builder.WithEnvironment(async context =>
            {
                string? browser = await builder.Resource.BrowserToken.GetValueAsync(context.CancellationToken).ConfigureAwait(false);
                string? otlp = await builder.Resource.OtlpApiKey.GetValueAsync(context.CancellationToken).ConfigureAwait(false);
                if (browser is null || otlp is null || browser.Length < 32 || otlp.Length < 32 || string.Equals(browser, otlp, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Dashboard credentials require distinct values of at least 32 characters; generate each with 128 bits or more entropy.");
                }

                if (otlp.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_')))
                {
                    throw new InvalidOperationException("The OTLP key must use URL-safe letters, digits, hyphens and underscores for exporter headers.");
                }

                context.EnvironmentVariables["Dashboard__Frontend__BrowserToken"] = builder.Resource.BrowserToken;
                context.EnvironmentVariables["Dashboard__Otlp__PrimaryApiKey"] = builder.Resource.OtlpApiKey;
            });
            builder.WithEnvironment("ASPNETCORE_URLS", "http://[::]:18888")
                .WithEnvironment("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true")
                .WithEnvironment("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL", "http://[::]:18889")
                .WithEnvironment("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL", "http://[::]:18890")
                .WithEnvironment("Dashboard__Frontend__AuthMode", "BrowserToken")
                .WithEnvironment("Dashboard__Otlp__AuthMode", "ApiKey")
                .WithEnvironment("Dashboard__Api__Disabled", "true")
                .WithEnvironment("Logging__LogLevel__Aspire.Dashboard.DashboardWebApplication", "Warning")
                .WithEnvironment("ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "false");
            RailwayBuilderExtensions.PublishToRailway(builder, target, service =>
            {
                service.ServiceName = options.ServiceName;
                service.OwnershipMode = options.OwnershipMode;
                service.ExistingServiceId = options.ExistingServiceId;
                service.Image = options.Image;
                service.Port = 18888;
                service.PublicDomain = true;
                service.HealthCheckPath = "/health";
                service.Region = options.Region;
                service.MemoryGB = options.MemoryGB;
                service.VCpus = options.VCpus;
                service.DeploymentTimeout = TimeSpan.FromSeconds(options.DeploymentTimeoutSeconds);
                if (options.CustomDomain is not null)
                {
                    service.CustomDomains.Add(options.CustomDomain);
                }
            });
            return builder;
        }

        /// <summary>Publishes a dashboard from a TypeScript AppHost.</summary>
        [AspireExport("pinguapps.railway.dashboard.publish", MethodName = "publishToRailway")]
        public IResourceBuilder<RailwayDashboardResource> PublishDashboardToRailway(
            IResourceBuilder<RailwayTargetResource> target, RailwayDashboardOptions? options = null) => builder.PublishToRailway(target, configured =>
            {
                if (options is not null)
                {
                    configured.ServiceName = options.ServiceName;
                    configured.OwnershipMode = options.OwnershipMode;
                    configured.ExistingServiceId = options.ExistingServiceId;
                    configured.Image = options.Image;
                    configured.Region = options.Region;
                    configured.CustomDomain = options.CustomDomain;
                    configured.MemoryGB = options.MemoryGB;
                    configured.VCpus = options.VCpus;
                    configured.DeploymentTimeoutSeconds = options.DeploymentTimeoutSeconds;
                }
            });

        /// <summary>Gets the token-free deployed frontend HTTPS URL as a deferred expression.</summary>
        [AspireExport("pinguapps.railway.dashboard.publicUrl", MethodName = "getRailwayDashboardUrl")]
        public ReferenceExpression GetRailwayDashboardUrl()
        {
            if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
            {
                return ReferenceExpression.Create($"{string.Empty}");
            }

            return ReferenceExpression.Create($"{builder.GetRailwayOutputs().PublicUrl}");
        }
    }

    /// <summary>Connects deployed workloads to private authenticated ingestion.</summary>
    extension<T>(IResourceBuilder<T> builder) where T : IResourceWithEnvironment
    {

        /// <summary>Routes logs, traces and metrics to the private dashboard only during deployment.</summary>
        [AspireExportIgnore(Reason = "TypeScript uses resource-specific exports.")]
        public IResourceBuilder<T> WithRailwayDashboard(IResourceBuilder<RailwayDashboardResource> dashboard)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(dashboard);
            if (builder.ApplicationBuilder.ExecutionContext.IsPublishMode)
            {
                builder.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", ReferenceExpression.Create($"http://{dashboard.GetRailwayOutputs().PrivateHostname}:18890"))
                    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf")
                    .WithEnvironment("OTEL_EXPORTER_OTLP_HEADERS", ReferenceExpression.Create($"x-otlp-api-key={dashboard.Resource.OtlpApiKey}"))
                    .WithAnnotation(new ResourceRelationshipAnnotation(dashboard.Resource, "Reference"));
            }

            return builder;
        }
    }

    /// <summary>Exports container OTLP wiring to guest AppHosts.</summary>
    extension(IResourceBuilder<ContainerResource> builder)
    {

        /// <summary>Routes a container's telemetry to the dashboard.</summary>
        [AspireExport("pinguapps.railway.dashboard.container.reference", MethodName = "withRailwayDashboard")]
        public IResourceBuilder<ContainerResource> WithContainerRailwayDashboard(IResourceBuilder<RailwayDashboardResource> dashboard) => builder.WithRailwayDashboard(dashboard);
    }

    /// <summary>Exports project OTLP wiring to guest AppHosts.</summary>
    extension(IResourceBuilder<ProjectResource> builder)
    {

        /// <summary>Routes a project's telemetry to the dashboard.</summary>
        [AspireExport("pinguapps.railway.dashboard.project.reference", MethodName = "withRailwayDashboard")]
        public IResourceBuilder<ProjectResource> WithProjectRailwayDashboard(IResourceBuilder<RailwayDashboardResource> dashboard) => builder.WithRailwayDashboard(dashboard);
    }
}
