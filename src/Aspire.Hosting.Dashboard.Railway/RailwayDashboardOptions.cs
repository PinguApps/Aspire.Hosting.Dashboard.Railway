namespace Aspire.Hosting.Dashboard.Railway;

/// <summary>Configures the single standalone diagnostic dashboard service.</summary>
[AspireDto]
public sealed class RailwayDashboardOptions
{
    /// <summary>Gets or sets the ownership policy, defaulting to proven creation or adoption.</summary>
    public Aspire.Hosting.Railway.RailwayOwnershipMode OwnershipMode { get; set; } = Aspire.Hosting.Railway.RailwayOwnershipMode.CreateOrAdopt;

    /// <summary>Gets or sets an existing service ID for explicit adoption of unmarked infrastructure.</summary>
    public string? ExistingServiceId { get; set; }

    /// <summary>Gets or sets the service name, defaulting to the Aspire resource name.</summary>
    public string? ServiceName { get; set; }

    /// <summary>Gets or sets an immutable dashboard image, defaulting to the tested 13.6 Linux amd64 image.</summary>
    public string Image { get; set; } = "mcr.microsoft.com/dotnet/aspire-dashboard@sha256:94f41463f93ff8f87257a9ae8681a190fcd73e8e10cb46884160a6a6a8321356";

    /// <summary>Gets or sets the deployment region.</summary>
    public string? Region { get; set; }

    /// <summary>Gets or sets the optional public HTTPS custom domain. DNS remains an operator responsibility.</summary>
    public string? CustomDomain { get; set; }

    /// <summary>Gets or sets the memory limit in GB.</summary>
    public double? MemoryGB { get; set; }

    /// <summary>Gets or sets the vCPU limit.</summary>
    public double? VCpus { get; set; }

    /// <summary>Gets or sets the deployment timeout in seconds.</summary>
    public int DeploymentTimeoutSeconds { get; set; } = 600;
}
