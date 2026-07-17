namespace HomelabDashboard.Data;

public sealed class StatCardData
{
    public required string Icon { get; init; }
    public required double Value { get; init; }
    public string Suffix { get; init; } = "";
    public required string Label { get; init; }
    public required string AccentToken { get; init; }
    public string? DeltaLabel { get; init; }
}

public sealed class DiskUsage
{
    public required int UsedPercent { get; init; }
    public required string Total { get; init; }
    public required string Used { get; init; }
    public required string Free { get; init; }
    public required string Mount { get; init; }
    public required string FsType { get; init; }
}

public sealed class DashboardStats
{
    public List<StatCardData> MediaStats { get; init; } = [];
    public List<StatCardData> NetworkStats { get; init; } = [];
    public required DiskUsage Disk { get; init; }
}

public interface IDashboardStatsProvider
{
    Task<DashboardStats> GetStatsAsync();
}

/// <summary>
/// Mocked dashboard numbers — a straight port of the static site's
/// hard-coded figures. Point this at Pi-hole's API, an exporter, or a
/// `df`/`du` shell-out later; nothing above this interface changes.
/// </summary>
public sealed class MockDashboardStatsProvider : IDashboardStatsProvider
{
    public Task<DashboardStats> GetStatsAsync()
    {
        var stats = new DashboardStats
        {
            MediaStats =
            [
                new StatCardData { Icon = "🎬", Value = 847, Label = "Total Movies", AccentToken = "teal" },
                new StatCardData { Icon = "📺", Value = 124, Label = "TV Shows", AccentToken = "purple", DeltaLabel = "↑ 3 this week" },
                new StatCardData { Icon = "🎵", Value = 2341, Label = "Audiobooks", AccentToken = "gold" },
                new StatCardData { Icon = "📚", Value = 418, Label = "eBooks", AccentToken = "green" },
                new StatCardData { Icon = "🎬", Value = 5, Label = "Movies Added", AccentToken = "coral", DeltaLabel = "↑ this week" },
                new StatCardData { Icon = "📺", Value = 18, Label = "Episodes Added", AccentToken = "teal", DeltaLabel = "↑ this week" },
            ],
            NetworkStats =
            [
                new StatCardData { Icon = "🛡️", Value = 14872, Label = "Ads Blocked", AccentToken = "green", DeltaLabel = "↑ today" },
                new StatCardData { Icon = "📊", Value = 23, Suffix = "%", Label = "Queries Blocked", AccentToken = "teal" },
                new StatCardData { Icon = "🔍", Value = 62144, Label = "DNS Queries", AccentToken = "gold", DeltaLabel = "↑ today" },
            ],
            Disk = new DiskUsage
            {
                UsedPercent = 68,
                Total = "4.0 TB",
                Used = "2.7 TB",
                Free = "1.3 TB",
                Mount = "/mnt/data",
                FsType = "ext4"
            }
        };

        return Task.FromResult(stats);
    }
}
