using System.Collections.Concurrent;

namespace Terces;

/// <summary>
/// Provides an in-memory implementation of <see cref="ISecretStore"/>.
/// </summary>
/// <param name="time">The <see cref="TimeProvider"/> used to get current time values.</param>
public class InMemorySecretStore(TimeProvider time) : ISecretStore
{
    /// <summary>
    /// The identifier for this in-memory store to use for secret metadata.
    /// </summary>
    private const string StoreId = "in-memory-store";

    /// <summary>
    /// Record to store a secret along with its metadata.
    /// </summary>
    /// <param name="Info">The metadata about the secret.</param>
    /// <param name="SecretValue">The actual secret value.</param>
    private record SecretEntry(SecretInfo Info, string SecretValue);

    /// <summary>
    /// Thread-safe dictionary to store secrets.
    /// </summary>
    private readonly ConcurrentDictionary<string, SecretEntry> _secrets = new();
    
    /// <summary>
    /// The time provider used to get current time values.
    /// </summary>
    private readonly TimeProvider _time = time;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemorySecretStore"/> class using the system time provider.
    /// </summary>
    public InMemorySecretStore() : this(TimeProvider.System)
    {
    }

    /// <summary>
    /// Retrieves the metadata for a secret by its name.
    /// </summary>
    /// <param name="name">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The secret's metadata if found; otherwise, null.</returns>
    public Task<SecretInfo?> GetSecretAsync(string name, CancellationToken cancellationToken)
    {
        var result = _secrets.GetValueOrDefault(name);
        return Task.FromResult(result?.Info);
    }

    /// <summary>
    /// Creates or updates a secret with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="value">The value of the secret.</param>
    /// <param name="expiresOn">The expiration date of the secret, if any.</param>
    /// <param name="contentType">The content type of the secret.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The metadata of the created or updated secret.</returns>
    public Task<SecretInfo?> UpdateSecretAsync(string name, string value, DateTimeOffset? expiresOn, string contentType,
        CancellationToken cancellationToken = default)
    {
        var result = _secrets.AddOrUpdate(name,
            s => new SecretEntry(
                new SecretInfo(name, name, contentType, true, _time.GetUtcNow(), expiresOn, _time.GetUtcNow(),
                    StoreId, "1"),
                value
            ),
            (s, entry) => new SecretEntry(
                new SecretInfo(name, name, contentType, entry.Info.Enabled, entry.Info.CreatedOn,
                    expiresOn, _time.GetUtcNow(), entry.Info.StoreId, entry.Info.Version),
                value
            )
        );

        return Task.FromResult<SecretInfo?>(result.Info);
    }
}