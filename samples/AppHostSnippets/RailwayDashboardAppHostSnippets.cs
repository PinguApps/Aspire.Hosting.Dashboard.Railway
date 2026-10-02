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
        IResourceBuilder<ContainerResource> worker = builder.AddContainer("worker", "ghcr.io/example/site-worker")
            .WithImageSHA256(new string('a', 64));
        if (builder.ExecutionContext.IsRunMode)
        {
            return;
        }

        IResourceBuilder<RailwayTargetResource> target = builder.AddRailwayTarget("railway", builder.AddParameter("railway-project-id", Current("Parameters__railway-project-id")),
            builder.AddParameter("railway-environment-id", Current("Parameters__railway-environment-id")), builder.AddParameter("railway-token", Current("Parameters__railway-token"), secret: true),
            builder.AddParameter("site-key", Current("Parameters__site-key")));
        IResourceBuilder<RailwayDashboardResource> dashboard = builder.AddRailwayDashboard("diagnostics", builder.AddParameter("dashboard-browser-token", Current("Parameters__dashboard-browser-token"), secret: true),
            builder.AddParameter("dashboard-otlp-key", Current("Parameters__dashboard-otlp-key"), secret: true)).PublishToRailway(target, (RailwayDashboardOptions options) =>
            {
                options.Region = "europe-west4";
                options.MemoryGB = 1;
                options.VCpus = 1;
            });
        worker.WithRailwayDashboard(dashboard).PublishToRailway(target);
    }

    private static string Current(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"Missing deployment input: {name}");
}
