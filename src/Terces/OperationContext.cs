namespace Terces;

public record OperationContext
{
    /// <summary>
    /// Gets the set of secret stores in the current operating context.
    /// </summary>
    public required IReadOnlyDictionary<string, ISecretStore> Stores { get; init; } =
        new Dictionary<string, ISecretStore>();

    public string SecretValue1 { get; set; } = "";
    public required bool Force { get; init; } = false;
    public required bool IsWhatIf { get; init; } = true;
}