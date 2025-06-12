namespace Terces;

/// <summary>
/// Represents the context for an operation, providing configuration settings,
/// credentials, secret stores, and other relevant context data for execution.
/// </summary>
public record OperationContext
{
    /// <summary>
    /// Represents a collection of secret stores accessible within the current operation context.
    /// </summary>
    public required IReadOnlyDictionary<string, ISecretStore> Stores { get; init; } =
        new Dictionary<string, ISecretStore>();

    /// <summary>
    /// Represents the collection of rotators keyed by strategy.
    /// </summary>
    public IReadOnlyDictionary<string, IRotator> Rotators { get; init; } = new Dictionary<string, IRotator>();

    /// <summary>
    /// Gets or sets the primary secret value used during the execution of an operation.
    /// </summary>
    public string SecretValue1 { get; set; } = "";

    /// <summary>
    /// Indicates whether to bypass standard checks, forcing certain operations to execute
    /// regardless of the usual eligibility criteria.
    /// </summary>
    public required bool Force { get; set; } = false;

    /// <summary>
    /// Indicates whether the operation is being executed in "what-if" mode.
    /// When set to true, the operation simulates the actions without making any actual changes.
    /// </summary>
    public required bool IsWhatIf { get; set; } = false;

    /// <summary>
    /// Gets the dictionary of credentials associated with the current operating context.
    /// </summary>
    public IReadOnlyDictionary<string, object> Credentials { get; init; } =
        new Dictionary<string, object>();
}