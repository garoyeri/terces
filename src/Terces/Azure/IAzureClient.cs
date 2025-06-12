namespace Terces.Azure;

/// <summary>
/// Represents the relevant details of a PostgreSQL Flexible Server instance retrieved from Azure.
/// </summary>
public record PostgreSqlFlexibleServerDetails(string Hostname, string Username);

/// <summary>
/// Represents a storage account access key, including its name and value.
/// </summary>
public record StorageAccountKey(string KeyName, string Value);

/// <summary>
/// Represents an interface for interacting with Azure resources.
/// Provides methods for managing PostgreSQL Flexible Servers and Storage Accounts.
/// </summary>
public interface IAzureClient
{
    /// <summary>Asynchronously retrieves the details of a PostgreSQL flexible server, such as its hostname and username, based on the provided resource ID.</summary>
    /// <param name="resourceId">
    /// The unique identifier of the PostgreSQL flexible server resource.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional token to observe cancellation requests.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an instance of
    /// <see cref="PostgreSqlFlexibleServerDetails"/> with the server's details, or null if the details cannot be retrieved.
    /// </returns>
    Task<PostgreSqlFlexibleServerDetails?> GetPostgreSqlFlexibleServerDetailsAsync(string resourceId, CancellationToken cancellationToken = default);

    /// <summary>Updates the administrator password for a PostgreSQL Flexible Server.</summary>
    /// <param name="resourceId">The resource identifier of the PostgreSQL Flexible Server.</param>
    /// <param name="password">The new administrator password to set for the server.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the password update was successful.</returns>
    Task<bool> UpdatePostgreSqlFlexibleServerAdminPasswordAsync(string resourceId, string password, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the expected storage account keys for the specified resource.</summary>
    /// <param name="resourceId">
    /// The unique identifier of the resource for which the storage account keys are being retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the ongoing operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an array of
    /// <see cref="StorageAccountKey"/> objects, each of which contains the key name and its associated value.
    /// </returns>
    Task<StorageAccountKey[]> GetExpectedStorageAccountKeys(string resourceId, CancellationToken cancellationToken);

    /// <summary>Rotates a storage account key for the specified resource and key name.</summary>
    /// <param name="resourceId">The resource ID of the storage account for which the key is to be rotated.</param>
    /// <param name="keyToRotate">The name of the key to be rotated (e.g., "key1" or "key2").</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="StorageAccountKey"/> object containing the details of the rotated key, or null if the rotation fails.</returns>
    Task<StorageAccountKey?> RotateStorageAccountKey(string resourceId, string keyToRotate, CancellationToken cancellationToken);
}