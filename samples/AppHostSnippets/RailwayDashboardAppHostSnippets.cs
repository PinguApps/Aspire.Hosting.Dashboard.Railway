using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dashboard.Railway;
using Aspire.Hosting.Railway;

namespace PinguApps.Aspire.Hosting.Dashboard.Railway.Samples;

/// <summary>Compile-validated AppHost setup.</summary>
public static class RailwayDashboardAppHostSnippets
{
    /// <summary>Declares the optional deployed diagnostic dashboard.</summary>
    public static void Configure(IDistributedApplicationBuilder builder)
    {
        IResourceBuilder<RailwayTargetResource> target = builder.AddRailwayTarget("railway", builder.AddParameter("railway-project-id"),
            builder.AddParameter("railway-environment-id"), builder.AddParameter("railway-token", secret: true),
            builder.AddParameter("site-key"));
        IResourceBuilder<RailwayDashboardResource> dashboard = builder.AddRailwayDashboard("diagnostics", builder.AddParameter("dashboard-browser-token", secret: true),
            builder.AddParameter("dashboard-otlp-key", secret: true)).PublishToRailway(target, (RailwayDashboardOptions options) =>
            {
                options.Region = "europe-west4";
                options.MemoryGB = 1;
                options.VCpus = 1;
            });
        builder.AddContainer("worker", "ghcr.io/example/site-worker")
            .WithImageSHA256(new string('a', 64))
            .WithRailwayDashboard(dashboard)
            .PublishToRailway(target);
    }
}
