using System.Text.Json;

namespace Terces.Azure;

/// <summary>
/// Represents a mechanism to handle the rotation of keys for Azure Storage Accounts, ensuring secure and periodic updates
/// of access credentials stored within a configured secret store.
/// </summary>
public class StorageAccountKeyRotator : IRotator
{
    private const string SecondarySecretSuffix = "Backup";
    
    /// <summary>
    /// Represents the time provider used for evaluating initialization and rotation candidacy,
    /// as well as determining expiration details within the rotator logic.
    /// </summary>
    /// <remarks>
    /// This variable is instantiated through the constructor of the <see cref="AbstractRotator"/> class.
    /// It enables time-related functionalities, such as getting the current UTC time for computation purposes.
    /// </remarks>
    private readonly TimeProvider _time;

    private readonly IAzureClient _client;

    /// <summary>
    /// Provides functionality to manage and rotate the shared access keys for an Azure Storage Account.
    /// The class is responsible for performing the initialization and rotation of the keys, ensuring
    /// secure and consistent management of credentials in the associated secret store.
    /// </summary>
    public StorageAccountKeyRotator(IAzureClient client, TimeProvider time)
    {
        _time = time;
        _client = client;
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

    /// <summary>
    /// Asynchronously initializes a secret resource based on its configuration and context, if initialization is required.
    /// </summary>
    /// <param name="resource">The configuration details of the resource to be initialized.</param>
    /// <param name="store">The secret store interface where the secret resides.</param>
    /// <param name="context">The operational context for this initialization operation.</param>
    /// <param name="cancellationToken">A token to cancel the initialization operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the initialization process as a <see cref="RotationResult"/>.</returns>
    public async Task<RotationResult> InitializeAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        var initialResult = await resource.EvaluateInitializationCandidacy(store, context, _time, cancellationToken);
        if (initialResult != null) return initialResult;
        
        return await PerformInitialization(resource, store, context, cancellationToken);
    }

    /// Executes the rotation logic for the specified resource configuration,
    /// using the provided secret store and operation context, while considering the given cancellation token.
    /// The method first evaluates if the resource is eligible for rotation
    /// and performs the actual rotation if required.
    /// <param name="resource">
    /// The configuration details of the resource, including its attributes needed for rotation evaluation and execution.
    /// </param>
    /// <param name="store">
    /// The secret store interface used to interact with storage and retrieval of secrets during rotation.
    /// </param>
    /// <param name="context">
    /// The operation context providing additional metadata or settings related to the ongoing operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token to observe for cancellation requests during the execution of the asynchronous task.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the result of the rotation process in the form of a RotationResult.
    /// </returns>
    public async Task<RotationResult> RotateAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        var initialResult = await resource.EvaluateRotationCandidacy(store, context, _time, cancellationToken);
        if (initialResult != null) return initialResult;
        
        return await PerformRotation(resource, store, context, cancellationToken);
    }

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
    private async Task<RotationResult> PerformInitialization(ResourceConfiguration resource, ISecretStore store, OperationContext context,
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
        var keys = await _client.GetExpectedStorageAccountKeys(resource.TargetResourceId, cancellationToken);

        // make sure we have two keys
        if (keys.Length != 2)
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
        var rotatedKey = await _client.RotateStorageAccountKey(resource.TargetResourceId, keyToRotate, cancellationToken);
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
    private async Task<RotationResult> PerformRotation(ResourceConfiguration resource, ISecretStore store, OperationContext context,
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
        var keys = await _client.GetExpectedStorageAccountKeys(resource.TargetResourceId, cancellationToken);

        // make sure we have two keys
        if (keys.Length != 2)
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
        var rotatedKey = await _client.RotateStorageAccountKey(resource.TargetResourceId, keyToRotate, cancellationToken);
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
}