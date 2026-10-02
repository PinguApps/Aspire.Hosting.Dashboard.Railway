using Xunit;

namespace PinguApps.Aspire.Hosting.Dashboard.Railway.Tests;

/// <summary>Optional read-only security checks against an already deployed test dashboard.</summary>
public sealed class RailwayDashboardLiveTests
{
    /// <summary>Gets whether an explicitly configured live dashboard is available.</summary>
    public static bool HasLiveDashboard => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_DASHBOARD_TEST_URL"));

    /// <summary>Only authenticated frontend users can access the deployed diagnostic viewer.</summary>
    [Fact(SkipUnless = nameof(HasLiveDashboard), Skip = "Set RAILWAY_DASHBOARD_TEST_URL to an existing test dashboard to run live checks.")]
    [Trait("Category", "live-railway")]
    public async Task UnauthenticatedFrontendAndApiAreProtected()
    {
        Uri url = new(Environment.GetEnvironmentVariable("RAILWAY_DASHBOARD_TEST_URL")!);
        Assert.Equal(Uri.UriSchemeHttps, url.Scheme);
        using HttpClient client = new();
        using HttpResponseMessage frontend = await client.GetAsync(url, TestContext.Current.CancellationToken);
        Assert.Equal("/login", frontend.RequestMessage!.RequestUri!.AbsolutePath);
        using HttpResponseMessage api = await client.GetAsync(new Uri(url, "/api/telemetry/resources"), TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, api.StatusCode);
        using ByteArrayContent empty = new([]);
        using HttpResponseMessage otlp = await client.PostAsync(new Uri(url, "/v1/traces"), empty, TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, otlp.StatusCode);
    }
}
