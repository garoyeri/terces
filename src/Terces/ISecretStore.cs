namespace Terces;

/// <summary>
/// Represents a contract for managing secrets, allowing retrieval and updates of secret metadata
/// through implementations of the interface.
/// </summary>
public interface ISecretStore
{
    /// <summary>
    /// Retrieves the metadata for a secret by its name.
    /// </summary>
    /// <param name="name">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The secret's metadata if found; otherwise, null.</returns>
    Task<SecretInfo?> GetSecretAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a secret with the specified details in the underlying secret store.
    /// </summary>
    /// <param name="name">The name of the secret to create or update.</param>
    /// <param name="value">The value of the secret.</param>
    /// <param name="expiresOn">The expiration date and time of the secret, or null if the secret does not expire.</param>
    /// <param name="contentType">The content type of the secret.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the metadata of the created or updated secret.</returns>
    Task<SecretInfo?> UpdateSecretAsync(string name, string value, DateTimeOffset? expiresOn, string contentType,
        CancellationToken cancellationToken);
}