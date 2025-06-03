using System.Text.Json;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

namespace Terces.Azure;

/// <summary>
/// Represents a mechanism to handle the rotation of keys for Azure Storage Accounts, ensuring secure and periodic updates
/// of access credentials stored within a configured secret store.
/// </summary>
public class StorageAccountKeyRotator : AbstractRotator, IRotator
{
    /// <summary>
    /// Represents an instance of the Azure Resource Manager client for interacting with Azure resources.
    /// </summary>
    /// <remarks>
    /// This client is used to perform resource management tasks such as retrieving or modifying resources in Azure.
    /// It is instantiated using the provided <see cref="TokenCredential"/> to authenticate requests.
    /// </remarks>
    private readonly ArmClient _client;

    /// <summary>
    /// Provides functionality to manage and rotate the shared access keys for an Azure Storage Account.
    /// The class is responsible for performing the initialization and rotation of the keys, ensuring
    /// secure and consistent management of credentials in the associated secret store.
    /// </summary>
    public StorageAccountKeyRotator(TokenCredential credential, TimeProvider time) : base(time)
    {
        _client = new ArmClient(credential);
    }

    /// <summary>
    /// Represents the unique strategy type identifier for the rotation mechanism.
    /// This property specifically indicates the type of resource and rotation strategy
    /// being implemented by the corresponding rotator class. It is used to match the
    /// rotator with a compatible resource and ensure the proper rotation behavior.
    /// </summary>
    public static string StrategyType => "azure/storage/account/key";

    /// <summary>
    /// Represents a credential for a storage account key. Contains the key name and its value.
    /// </summary>
    public record StorageAccountKeyCredential(string name, string value);

    /// Responsible for performing the initialization process for storage account key rotation.
    /// This includes validating resource configurations, regenerating the specified key,
    /// and updating the secret in the provided secret store.
    /// <param name="resource">
    /// The resource configuration containing details about the target resource for key rotation.
    /// </param>
    /// <param name="store">
    /// The secret store where the updated secret will be stored.
    /// </param>
    /// <param name="context">
    /// The operation context providing additional metadata associated with the rotation process.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests during the initialization operation.
    /// </param>
    /// <return>
    /// A task representing the asynchronous operation, with a result of type <see cref="RotationResult"/>
    /// indicating the outcome of the initialization process.
    /// </return>
    protected override async Task<RotationResult> PerformInitialization(ResourceConfiguration resource, ISecretStore store, OperationContext context,
        CancellationToken cancellationToken)
    {
        if (resource.TargetResourceId == null)
        {
            return new RotationResult()
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Target resource ID is required for {StrategyType} rotation"
            };
        }

        // get the list of keys from the storage account
        var storageAccountResourceId = ResourceIdentifier.Parse(resource.TargetResourceId);
        var account = _client.GetStorageAccountResource(storageAccountResourceId);
        var keys = await GetExpectedStorageAccountKeys(account, cancellationToken);

        // make sure we have two keys
        if (keys.Count != 2)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Missing a key (key1 and key2 are required) for {resource.Name} in store {resource.StoreName}"
            };
        }

        // initialize to the first key
        const string keyToRotate = "key1";

        // rotate the desired key, grab its value and then rotate the secret
        var regenerateResponse = account.RegenerateKeyAsync(
            new StorageAccountRegenerateKeyContent(keyToRotate),
            cancellationToken);
        StorageAccountKey? rotatedKey = null;
        await foreach (var key in regenerateResponse)
        {
            if (key.KeyName != keyToRotate) continue;

            rotatedKey = key;
            break;
        }

        // make sure the rotated key was returned
        if (rotatedKey == null)
        {
            return new RotationResult()
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to rotate key {keyToRotate} for {resource.Name} in store {resource.StoreName}"
            };
        }
        
        // update the secret
        var json = JsonSerializer.Serialize(new StorageAccountKeyCredential(rotatedKey.KeyName, rotatedKey.Value));
        var updatedSecret = await store.UpdateSecretAsync(resource.Name, json, _time.GetUtcNow().AddDays(resource.ExpirationDays),
            "application/json", cancellationToken);
        if (updatedSecret == null)
        {
            // something has gone horribly wrong
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Regenerated key, but failed to update secret for {resource.Name} in store {resource.StoreName}. Reinitialization will be required to recover."
            };
        }

        return new RotationResult()
        {
            Name = resource.Name,
            WasRotated = true,
            Notes = $"Rotated key {keyToRotate} for {resource.Name} in store {resource.StoreName}"
        };
    }

    /// <summary>
    /// Performs the rotation of a storage account key for a specified resource.
    /// The method determines which key to rotate, regenerates it, updates the secret,
    /// and returns the result of the rotation operation.
    /// </summary>
    /// <param name="resource">
    /// The resource configuration containing details about the target resource requiring key rotation.
    /// </param>
    /// <param name="store">
    /// The secret store where the key-related secrets are managed.
    /// </param>
    /// <param name="context">
    /// The operation context providing metadata related to the rotation process.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token to monitor for cancellation requests.
    /// </param>
    /// <return>
    /// A <see cref="RotationResult"/> indicating the outcome of the rotation, including its status and any relevant notes.
    /// </return>
    protected override async Task<RotationResult> PerformRotation(ResourceConfiguration resource, ISecretStore store, OperationContext context,
        CancellationToken cancellationToken)
    {
        if (resource.TargetResourceId == null)
        {
            return new RotationResult()
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Target resource ID is required for {StrategyType} rotation"
            };
        }

        // get the list of keys from the storage account
        var storageAccountResourceId = ResourceIdentifier.Parse(resource.TargetResourceId);
        var account = _client.GetStorageAccountResource(storageAccountResourceId);
        var keys = await GetExpectedStorageAccountKeys(account, cancellationToken);

        // make sure we have two keys
        if (keys.Count != 2)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Missing a key (key1 and key2 are required) for {resource.Name} in store {resource.StoreName}"
            };
        }
        
        // figure out which one needs to be rotated: we need to check to see which key shows up in the secret value.
        var secretValue = await store.GetSecretValueAsync(resource.Name, cancellationToken);
        if (secretValue == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to retrieve secret value for {resource.Name} in store {resource.StoreName}"
            };
        }
        var secret = JsonSerializer.Deserialize<StorageAccountKeyCredential>(secretValue);
        if (secret == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to deserialize secret value for {resource.Name} in store {resource.StoreName}"
            };
        }
        
        // choose the opposite key to the one being used to be rotated in
        var keyToRotate = secret.name == "key1" ? "key2" : "key1";
        
        // rotate the desired key, grab its value and then rotate the secret
        var regenerateResponse = account.RegenerateKeyAsync(
            new StorageAccountRegenerateKeyContent(keyToRotate),
            cancellationToken);
        StorageAccountKey? rotatedKey = null;
        await foreach (var key in regenerateResponse)
        {
            if (key.KeyName != keyToRotate) continue;

            rotatedKey = key;
            break;
        }

        // make sure the rotated key was returned
        if (rotatedKey == null)
        {
            return new RotationResult()
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to rotate key {keyToRotate} for {resource.Name} in store {resource.StoreName}"
            };
        }
        
        // update the secret
        var json = JsonSerializer.Serialize(new StorageAccountKeyCredential(rotatedKey.KeyName, rotatedKey.Value));
        var updatedSecret = await store.UpdateSecretAsync(resource.Name, json, _time.GetUtcNow().AddDays(resource.ExpirationDays),
            "application/json", cancellationToken);
        if (updatedSecret == null)
        {
            // something has gone horribly wrong
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Regenerated key, but failed to update secret for {resource.Name} in store {resource.StoreName}. Reinitialization will be required to recover."
            };
        }

        return new RotationResult()
        {
            Name = resource.Name,
            WasRotated = true,
            Notes = $"Rotated key {keyToRotate} for {resource.Name} in store {resource.StoreName}"
        };
    }

    /// <summary>
    /// Retrieves the expected storage account keys for the specified storage account,
    /// selecting the keys intended for rotation.
    /// </summary>
    /// <param name="account">The storage account resource for which the keys should be retrieved.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A list containing the storage account keys "key1" and "key2" if found.</returns>
    private async Task<List<StorageAccountKey>> GetExpectedStorageAccountKeys(StorageAccountResource account,
        CancellationToken cancellationToken)
    {
        var keysResponse = account.GetKeysAsync(cancellationToken: cancellationToken);
        List<StorageAccountKey> keys = new();
        await foreach (var key in keysResponse)
        {
            // only choose the two keys that we want to rotate
            if (key.KeyName is "key1" or "key2")
                keys.Add(key);
        }

        return keys;
    }
}