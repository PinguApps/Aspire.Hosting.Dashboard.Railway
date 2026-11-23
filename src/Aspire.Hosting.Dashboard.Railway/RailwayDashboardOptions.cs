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

    /// <summary>Gets or sets an immutable dashboard image, defaulting to the tested Aspire 17.0.0 image.</summary>
    public string Image { get; set; } = "mcr.microsoft.com/aspire/dashboard@sha256:69b9b095610997cc2b5a3c265acd5c9f8d6b6b5040ef8cfdccc8a66efff0252d";

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
