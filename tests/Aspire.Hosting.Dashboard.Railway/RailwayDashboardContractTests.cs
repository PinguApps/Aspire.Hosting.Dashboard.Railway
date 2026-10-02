using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dashboard.Railway;
using Aspire.Hosting.Railway;
using Xunit;

namespace PinguApps.Aspire.Hosting.Dashboard.Railway.Tests;

/// <summary>Local and publish-time dashboard contracts.</summary>
public sealed class RailwayDashboardContractTests
{
    /// <summary>The documented local sample requires no deployment parameters.</summary>
    [Fact]
    public void LocalSampleDeclaresOnlyItsWorkload()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        Samples.RailwayDashboardAppHostSnippets.Configure(builder);
        Assert.IsType<ContainerResource>(Assert.Single(builder.Resources));
    }

    /// <summary>Credentials must remain explicit, secret and separate.</summary>
    [Fact]
    public void CredentialsRequireDistinctSecretParameters()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        IResourceBuilder<ParameterResource> secret = builder.AddParameter("secret", secret: true);
        IResourceBuilder<ParameterResource> plain = builder.AddParameter("plain");
        Assert.Throws<ArgumentException>(() => builder.AddRailwayDashboard("dashboard", secret, secret));
        Assert.Throws<ArgumentException>(() => builder.AddRailwayDashboard("dashboard", secret, plain));
    }

    /// <summary>Normal run mode leaves workload telemetry and local dashboard behaviour alone.</summary>
    [Fact]
    public void LocalWorkloadReceivesNoRemoteTelemetryConfiguration()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        IResourceBuilder<RailwayDashboardResource> dashboard = builder.AddRailwayDashboard("dashboard", builder.AddParameter("browser", secret: true), builder.AddParameter("otlp", secret: true));
        IResourceBuilder<RailwayTargetResource> target = builder.AddRailwayTarget("railway", builder.AddParameter("project"), builder.AddParameter("environment"),
            builder.AddParameter("token", secret: true), builder.AddParameter("site"));
        Assert.Same(dashboard, dashboard.PublishToRailway(target));
        IResourceBuilder<ContainerResource> workload = builder.AddContainer("worker", "image");
        Assert.Same(workload, workload.WithRailwayDashboard(dashboard));
        Assert.Empty(workload.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>());
        Assert.Empty(dashboard.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>());
        Assert.True(dashboard.Resource.IsExcludedFromPublish());
        Assert.Single(dashboard.Resource.Annotations.OfType<ExplicitStartupAnnotation>());
        Assert.Equal(string.Empty, dashboard.GetRailwayDashboardUrl().ValueExpression);
    }

    /// <summary>Timeouts are bounded before deployment.</summary>
    [Fact]
    public void NonPositiveTimeoutFails()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        IResourceBuilder<RailwayDashboardResource> dashboard = builder.AddRailwayDashboard("dashboard", builder.AddParameter("browser", secret: true), builder.AddParameter("otlp", secret: true));
        IResourceBuilder<RailwayTargetResource> target = builder.AddRailwayTarget("railway", builder.AddParameter("project"), builder.AddParameter("environment"),
            builder.AddParameter("token", secret: true), builder.AddParameter("site"));
        Assert.Throws<ArgumentOutOfRangeException>(() => dashboard.PublishToRailway(target, options => options.DeploymentTimeoutSeconds = 0));
    }

    /// <summary>Deployment config separates auth, disables APIs and suppresses token-bearing startup logs.</summary>
    [Fact]
    public async Task PublishedDashboardIsHardenedAndKeepsSecretsAsReferences()
    {
        IDistributedApplicationBuilder builder = CreatePublishBuilder();
        IResourceBuilder<ParameterResource> browser = builder.AddParameter("browser", new string('a', 32), secret: true);
        IResourceBuilder<ParameterResource> otlp = builder.AddParameter("otlp", new string('b', 32), secret: true);
        IResourceBuilder<RailwayDashboardResource> dashboard = builder.AddRailwayDashboard("dashboard", browser, otlp).PublishToRailway(CreateTarget(builder));
        Dictionary<string, object> environment = await GetEnvironmentAsync(builder, dashboard.Resource);
        Assert.Equal("BrowserToken", environment["Dashboard__Frontend__AuthMode"]);
        Assert.Equal("ApiKey", environment["Dashboard__Otlp__AuthMode"]);
        Assert.Equal("true", environment["Dashboard__Api__Disabled"]);
        Assert.Equal("false", environment["ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS"]);
        Assert.Equal("Warning", environment["Logging__LogLevel__Aspire.Dashboard.DashboardWebApplication"]);
        Assert.Same(browser.Resource, environment["Dashboard__Frontend__BrowserToken"]);
        Assert.Same(otlp.Resource, environment["Dashboard__Otlp__PrimaryApiKey"]);
        Assert.Equal("http://[::]:18889", environment["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"]);
        Assert.Equal("http://[::]:18890", environment["ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"]);
        Assert.False(dashboard.Resource.RequiresImageBuild());
    }

    /// <summary>Unsafe credential values fail before this dashboard applies remote configuration.</summary>
    [Theory]
    [InlineData("short", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb,")]
    public async Task UnsafeCredentialValuesFail(string browser, string otlp)
    {
        IDistributedApplicationBuilder builder = CreatePublishBuilder();
        IResourceBuilder<RailwayDashboardResource> dashboard = builder.AddRailwayDashboard("dashboard",
            builder.AddParameter("browser", browser, secret: true), builder.AddParameter("otlp", otlp, secret: true)).PublishToRailway(CreateTarget(builder));
        await Assert.ThrowsAsync<InvalidOperationException>(() => GetEnvironmentAsync(builder, dashboard.Resource));
    }

    /// <summary>Workloads reference private outputs and a secret header rather than public ingestion.</summary>
    [Fact]
    public async Task PublishedWorkloadUsesPrivateAuthenticatedHttpProtobuf()
    {
        IDistributedApplicationBuilder builder = CreatePublishBuilder();
        IResourceBuilder<RailwayDashboardResource> dashboard = builder.AddRailwayDashboard("dashboard",
            builder.AddParameter("browser", secret: true), builder.AddParameter("otlp", secret: true)).PublishToRailway(CreateTarget(builder));
        IResourceBuilder<ContainerResource> workload = builder.AddContainer("worker", "image").WithRailwayDashboard(dashboard);
        Dictionary<string, object> environment = await GetEnvironmentAsync(builder, workload.Resource);
        Assert.Equal("http/protobuf", environment["OTEL_EXPORTER_OTLP_PROTOCOL"]);
        ReferenceExpression endpoint = Assert.IsType<ReferenceExpression>(environment["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        Assert.Contains("railway.privateHostname", endpoint.ValueExpression, StringComparison.Ordinal);
        Assert.Contains(":18890", endpoint.ValueExpression, StringComparison.Ordinal);
        ReferenceExpression header = Assert.IsType<ReferenceExpression>(environment["OTEL_EXPORTER_OTLP_HEADERS"]);
        Assert.Contains("x-otlp-api-key=", header.ValueExpression, StringComparison.Ordinal);
        Assert.Same(dashboard.Resource.OtlpApiKey, Assert.Single(header.ValueProviders));
        Assert.Contains(workload.Resource.Annotations.OfType<ResourceRelationshipAnnotation>(), relationship => relationship.Resource == dashboard.Resource && relationship.Type == "Reference");
    }

    private static IDistributedApplicationBuilder CreatePublishBuilder() => DistributedApplication.CreateBuilder(new DistributedApplicationOptions
    {
        Args = ["--publisher", "manifest"],
        DisableDashboard = true,
    });

    private static IResourceBuilder<RailwayTargetResource> CreateTarget(IDistributedApplicationBuilder builder) => builder.AddRailwayTarget("railway",
        builder.AddParameter("project"), builder.AddParameter("environment"), builder.AddParameter("token", secret: true), builder.AddParameter("site"));

    private static async Task<Dictionary<string, object>> GetEnvironmentAsync(IDistributedApplicationBuilder builder, IResourceWithEnvironment resource)
    {
        Dictionary<string, object> environment = [];
        EnvironmentCallbackContext context = new(builder.ExecutionContext, resource, environment, CancellationToken.None);
        foreach (EnvironmentCallbackAnnotation annotation in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
        {
            await annotation.Callback(context);
        }

        return environment;
    }
}
