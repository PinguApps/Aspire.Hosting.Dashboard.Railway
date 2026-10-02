using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Dashboard.Railway;

/// <summary>A publish-only standalone dashboard with separate frontend and ingestion credentials.</summary>
public sealed class RailwayDashboardResource : ContainerResource
{
    /// <summary>Creates the dashboard resource.</summary>
    public RailwayDashboardResource(string name, ParameterResource browserToken, ParameterResource otlpApiKey)
        : base(name)
    {
        BrowserToken = browserToken;
        OtlpApiKey = otlpApiKey;
    }

    /// <summary>Gets the secret frontend login parameter.</summary>
    [AspireExportIgnore(Reason = "Credentials are passed as secret parameters at creation.")]
    public ParameterResource BrowserToken { get; }

    /// <summary>Gets the separate secret OTLP ingestion parameter.</summary>
    [AspireExportIgnore(Reason = "Credentials are passed as secret parameters at creation.")]
    public ParameterResource OtlpApiKey { get; }
}
