namespace Terces;

public record ResourceConfiguration
{
    public required string Name { get; init; }
    public required string StrategyType { get; init; }
    public required string StoreName { get; init; }
    public double ExpirationDays { get; set; } = 90.0;
    public double ExpirationOverlapDays { get; set; } = 0.0;
    public string ContentType { get; set; } = "text/plain";
}