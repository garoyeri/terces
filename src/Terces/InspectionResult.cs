namespace Terces;

/// <summary>
/// Represents the result of a secret inspection.
/// </summary>
/// <remarks>
/// This record includes information about the inspection, such as its name, strategy,
/// optionally associated secret metadata, additional notes, and an optional resource identifier.
/// </remarks>
public record InspectionResult
{
    /// <summary>
    /// Gets the secret name associated with the inspection result.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the rotation strategy associated with the inspection result.
    /// </summary>
    public required string Strategy { get; init; }

    /// <summary>
    /// Represents metadata about a secret associated with the inspection result.
    /// </summary>
    public SecretInfo? Secret { get; init; } = null;

    /// <summary>
    /// Gets or initializes informational or descriptive notes associated with the inspection result.
    /// </summary>
    /// <remarks>
    /// This property is designed to store additional context, comments, or metadata
    /// that provide further insights or explanations regarding the inspection result.
    /// By default, this property is initialized to an empty string.
    /// </remarks>
    public string Notes { get; init; } = "";

    /// <summary>
    /// Represents the identifier of a specific resource associated with the inspection result.
    /// </summary>
    public string? ResourceId { get; init; } = null;
}