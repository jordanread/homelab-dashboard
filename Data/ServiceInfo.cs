namespace HomelabDashboard.Data;

/// <summary>
/// One block of content in a service's detail page — mirrors the
/// original static .detail-section markup (heading + paragraphs/list/code/tip).
/// </summary>
public sealed class DetailSection
{
    public required string Heading { get; init; }
    public List<string> Paragraphs { get; init; } = [];
    public List<string>? BulletList { get; init; }
    public List<string>? NumberedList { get; init; }
    public string? CodeBlock { get; init; }
    public string? TipLabel { get; init; }
    public string? TipBody { get; init; }
}

/// <summary>
/// A single OS/platform tab under the certificate-trust instructions.
/// </summary>
public sealed class OsInstructions
{
    public required string OsKey { get; init; }
    public required string Label { get; init; }
    public List<string>? Steps { get; init; }
    public string? CodeBlock { get; init; }
    public string? Note { get; init; }
}

/// <summary>
/// Status shown on a service card / detail hero.
/// </summary>
public enum ServiceStatus
{
    Running,
    Connected,
    Degraded,
    Offline
}

/// <summary>
/// A running "container" on the deck — powers both the service card grid
/// on the home page and the /services/{slug} detail page.
/// </summary>
public sealed class ServiceInfo
{
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public required string Host { get; init; }
    public string? LaunchUrl { get; init; }
    public required string CardDescription { get; init; }
    public required string HeroDescription { get; init; }
    public ServiceStatus Status { get; init; } = ServiceStatus.Running;
    /// <summary>One of the accent tokens: teal, gold, coral, green, purple.</summary>
    public required string AccentToken { get; init; }
    public List<DetailSection> Sections { get; init; } = [];
}
