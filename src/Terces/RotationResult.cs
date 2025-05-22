namespace Terces;

/// <summary>
/// Represents the result of a rotation operation, containing details
/// about the rotation status, the target of the operation, and any
/// additional notes.
/// </summary>
public record RotationResult
{
    /// <summary>
    /// Gets the name associated with the rotation result.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Indicates whether the entity associated with the result was successfully rotated.
    /// </summary>
    public required bool WasRotated { get; init; }

    /// <summary>
    /// Provides additional information or remarks about the rotation outcome.
    /// This property can be used to store any notes or comments relevant to the specific result.
    /// </summary>
    public string Notes { get; init; } = "";
}