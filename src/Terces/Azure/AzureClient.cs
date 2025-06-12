using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Azure.ResourceManager.PostgreSql.FlexibleServers.Models;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

namespace Terces.Azure;

/// <summary>
/// Provides functionality for interacting with Azure resources,
/// specifically for managing PostgreSQL Flexible Servers and Storage Accounts.
/// </summary>
public class AzureClient : IAzureClient
{
    /// <summary>
    /// A private instance of the Azure Resource Manager (ARM) client used to interact with Azure resources.
    /// This client is used internally to manage and retrieve information for resources such as PostgreSQL Flexible Servers
    /// and Azure Storage Accounts.
    /// </summary>
    private readonly ArmClient _client;

    /// <summary>
    /// Provides methods to interact with Azure resources such as PostgreSQL Flexible Servers
    /// and Storage Accounts. This class is an implementation of the <see cref="IAzureClient"/> interface.
    /// </summary>
    public AzureClient(TokenCredential credential)
    {
        _client = new ArmClient(credential);
    }

    /// <summary>
    /// Retrieves the details of a PostgreSQL Flexible Server instance from Azure, including its fully-qualified domain name
    /// and administrator login username.
    /// </summary>
    /// <param name="resourceId">The resource ID of the PostgreSQL Flexible Server instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="PostgreSqlFlexibleServerDetails"/> object containing the server's hostname and username
    /// if the server details are successfully retrieved; otherwise, null.
    /// </returns>
    public async Task<PostgreSqlFlexibleServerDetails?> GetPostgreSqlFlexibleServerDetailsAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _client.GetPostgreSqlFlexibleServerResource(ResourceIdentifier.Parse(resourceId));
            var details = await server.GetAsync(cancellationToken);
            if (details == null) return null;

            return new PostgreSqlFlexibleServerDetails(
                details.Value.Data.FullyQualifiedDomainName,
                details.Value.Data.AdministratorLogin);
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }

    /// <summary>
    /// Updates the administrator password for a PostgreSQL Flexible Server in Azure.
    /// </summary>
    /// <param name="resourceId">The resource ID of the PostgreSQL Flexible Server.</param>
    /// <param name="password">The new administrator password to be set for the server.</param>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a boolean value indicating whether the password update was successful.
    /// </returns>
    public async Task<bool> UpdatePostgreSqlFlexibleServerAdminPasswordAsync(string resourceId, string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _client.GetPostgreSqlFlexibleServerResource(ResourceIdentifier.Parse(resourceId));
            var patch = new PostgreSqlFlexibleServerPatch
            {
                AdministratorLoginPassword = password
            };

            await server.UpdateAsync(WaitUntil.Completed, patch, cancellationToken);

            return true;
        }
        catch (RequestFailedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves the expected storage account keys based on the specified resource ID.
    /// </summary>
    /// <param name="resourceId">The resource ID of the storage account.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An array of storage account keys that are expected to be rotated. If no keys are found or an error occurs, an empty array is returned.</returns>
    public async Task<StorageAccountKey[]> GetExpectedStorageAccountKeys(string resourceId, CancellationToken cancellationToken)
    {
        var storageAccountResourceId = ResourceIdentifier.Parse(resourceId);
        var account = _client.GetStorageAccountResource(storageAccountResourceId);

        try
        {
            var keysResponse = account.GetKeysAsync(cancellationToken: cancellationToken);
            StorageAccountKey? key1 = null, key2 = null;
            await foreach (var key in keysResponse)
            {
                // only choose the two keys that we want to rotate
                if (key.KeyName == "key1")
                    key1 = new StorageAccountKey(key.KeyName, key.Value);
                else if (key.KeyName == "key2")
                    key2 = new StorageAccountKey(key.KeyName, key.Value);
            }

            if (key1 == null && key2 != null) return [key2];
            if (key2 == null && key1 != null) return [key1];
            if (key1 != null && key2 != null) return [key1, key2];
            return [];
        }
        catch (RequestFailedException)
        {
            return [];
        }
    }

    /// <summary>
    /// Rotates a specific access key for an Azure Storage Account.
    /// </summary>
    /// <param name="resourceId">The resource ID of the Azure Storage Account.</param>
    /// <param name="keyToRotate">The name of the key to rotate.</param>
    /// <param name="cancellationToken">A token to observe while waiting for task completion.</param>
    /// <returns>A <see cref="StorageAccountKey"/> representing the rotated key on success, or null if the operation fails.</returns>
    public async Task<StorageAccountKey?> RotateStorageAccountKey(string resourceId, string keyToRotate, CancellationToken cancellationToken)
    {
        var storageAccountResourceId = ResourceIdentifier.Parse(resourceId);
        var account = _client.GetStorageAccountResource(storageAccountResourceId);

        try
        {
            var regenerateResponse = account.RegenerateKeyAsync(
                new StorageAccountRegenerateKeyContent(keyToRotate),
                cancellationToken);
            await foreach (var key in regenerateResponse)
            {
                if (key.KeyName != keyToRotate) continue;

                return new StorageAccountKey(key.KeyName, key.Value);
            }

            return null;
        }
        catch (RequestFailedException)
        {
            return null;
        }
    }
}