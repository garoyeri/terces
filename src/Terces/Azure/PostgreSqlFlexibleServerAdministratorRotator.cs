using System.Text.Json;

namespace Terces.Azure;

/// A rotator specifically designed for rotating the administrator password of an Azure PostgreSQL Flexible Server.
/// This class interacts with Azure's ARM client to facilitate secure credential rotation and updates stored credentials.
/// Inherits from the abstract base class `AbstractRotator` and implements the `IRotator` interface.
/// Executes the rotation process. Generates a new administrative password for the PostgreSQL Flexible Server,
/// applies the new password to the server, and updates the secret in the provided secret store.
public class PostgreSqlFlexibleServerAdministratorRotator: AbstractRotator, IRotator
{
    private readonly IAzureClient _client;

    /// Represents a rotator that performs password rotation for PostgreSQL Flexible Server administrators.
    /// This class extends the functionality of AbstractRotator and implements the IRotator interface.
    public PostgreSqlFlexibleServerAdministratorRotator(IAzureClient client, TimeProvider time) : base(time)
    {
        _client = client;
    }

    /// <summary>
    /// Represents the type of rotation strategy used for the resource.
    /// </summary>
    /// <remarks>
    /// This property specifies the unique identifier of the strategy for rotating
    /// administrator credentials in a flexible server PostgreSQL environment.
    /// It is mainly utilized to differentiate this rotation type from other
    /// strategies within the system.
    /// </remarks>
    public static string StrategyType => "azure/postgresql/flexible-server/administrator";

    /// Represents the credentials for an Azure PostgreSQL Flexible Server administrator.
    /// Encapsulates the server's hostname, username, and password.
    /// Used during the credential rotation process to securely store and update
    /// the administrative access information for the database server.
    public record Credential(string hostname, string username, string password);

    /// <summary>
    /// Performs the initialization logic for the rotation process by delegating
    /// the operation to the rotation method.
    /// </summary>
    /// <param name="resource">The resource configuration to be initialized, including its name, strategy type, and other metadata.</param>
    /// <param name="store">The secret store where the resource's secrets are managed.</param>
    /// <param name="context">The operation context that provides information such as credentials and flags like Force or IsWhatIf.</param>
    /// <param name="cancellationToken">The cancellation token to observe for operation cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the details of the rotation process, including whether it was successful.</returns>
    protected override Task<RotationResult> PerformInitialization(ResourceConfiguration resource, ISecretStore store, OperationContext context,
        CancellationToken cancellationToken)
    {
        return PerformRotation(resource, store, context, cancellationToken);
    }

    /// <summary>
    /// Performs the rotation operation for a PostgreSQL Flexible Server administrator password.
    /// Updates the server credentials and stores the new credentials in the specified secret store.
    /// </summary>
    /// <param name="resource">The resource configuration containing details about the target resource to be rotated.</param>
    /// <param name="store">The secret store where the updated credentials will be stored.</param>
    /// <param name="context">The context for the operation, including any operation-specific flags or metadata.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="RotationResult"/> indicating the outcome of the rotation operation, including whether the rotation was successful.</returns>
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

        var serverDetails = await _client.GetPostgreSqlFlexibleServerDetailsAsync(resource.TargetResourceId, cancellationToken);
        if (serverDetails == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to retrieve server details for {resource.Name} in store {resource.StoreName}"
            };
        }
        
        if (context.IsWhatIf)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = true,
                Notes = $"Would have rotated administrator password for {resource.Name} in store {resource.StoreName}"
            };
        }
        
        // rotate the server credential
        var newPassword = PasswordGenerator.Generate(16);
        await _client.UpdatePostgreSqlFlexibleServerAdminPasswordAsync(resource.TargetResourceId, newPassword, cancellationToken);
        
        // store the new credential
        var serverCredentials = new Credential(serverDetails.Hostname, serverDetails.Username, newPassword);
        
        var json = JsonSerializer.Serialize(serverCredentials);
        var updatedSecret = await store.UpdateSecretAsync(resource.Name, json, _time.GetUtcNow().AddDays(resource.ExpirationDays), "application/json", cancellationToken);
        if (updatedSecret == null)
        {
            // something has gone horribly wrong
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to update secret for {resource.Name} in store {resource.StoreName}. Reinitialization will be required to recover."
            };
        }
        
        return new RotationResult()
        {
            Name = resource.Name,
            WasRotated = true,
            Notes = $"Rotated administrator password for {resource.Name} in store {resource.StoreName}"
        };
    }
}