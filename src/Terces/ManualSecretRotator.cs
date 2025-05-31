namespace Terces;

/// <summary>
/// A rotator that updates a secret only in the secret store without linking it
/// to any external resource. This is useful in scenarios where permissions
/// to modify secrets on target resources are unavailable or when the secret
/// has already been updated externally and needs to be synced with the store.
/// </summary>
public class ManualSecretRotator : AbstractRotator, IRotator
{
    /// <summary>
    /// Represents a manual implementation of a secret rotation process,
    /// where the secret rotation is initiated by manual intervention or trigger,
    /// rather than an automated mechanism.
    /// </summary>
    /// <remarks>
    /// This class provides functionality to initialize a secret or skip initialization
    /// if it is already configured, adhering to a manual rotation strategy.
    /// It can interact with resources, securely store secrets, and track their expiration periods.
    /// </remarks>
    public ManualSecretRotator(TimeProvider time) : base(time)
    {
    }

    /// <summary>
    /// Represents the strategy type identifier for the manual/generic secret rotation mechanism.
    /// </summary>
    /// <remarks>
    /// Used to define and categorize the rotation strategy implemented by the ManualSecretRotator class.
    /// </remarks>
    public static string StrategyType => "manual/generic";

    /// <summary>
    /// Performs the initialization process for the resource, interacting with the secret store
    /// and applying the operational context as part of the rotation logic.
    /// </summary>
    /// <param name="resource">The resource configuration containing details about the target resource for initialization.</param>
    /// <param name="store">The secret store instance used for managing and accessing secrets.</param>
    /// <param name="context">The operational context carrying metadata and instructions for the operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the initialization process.</param>
    /// <returns>Returns a task representing the asynchronous operation, with a result of type <see cref="RotationResult"/>.</returns>
    protected override Task<RotationResult> PerformInitialization(ResourceConfiguration resource, ISecretStore store, OperationContext context,
        CancellationToken cancellationToken)
    {
        return PerformRotation(resource, store, context, cancellationToken);
    }

    /// Performs the rotation of a secret based on the given resource configuration, secret store, and operation context.
    /// <param name="resource">The configuration details for the resource whose secret is being rotated.</param>
    /// <param name="store">The secret store where the secret is managed.</param>
    /// <param name="context">The operational context for the secret rotation, including relevant values and flags.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the rotation process.</returns>
    protected override async Task<RotationResult> PerformRotation(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        var newExpiration = _time.GetUtcNow().AddDays(resource.ExpirationDays);
        var newSecretValue = context.SecretValue1;
        
        if (context.IsWhatIf)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = true,
                Notes = $"Would have rotated secret {resource.Name} in store {resource.StoreName}"
            };
        }
        
        var result = await store.UpdateSecretAsync(resource.Name, newSecretValue, newExpiration, resource.ContentType,
            cancellationToken);
        if (result == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to rotate secret {resource.Name} in store {resource.StoreName}"
            };
        }

        return new RotationResult()
        {
            Name = resource.Name,
            WasRotated = true,
            Notes = $"Rotated secret {resource.Name} in store {resource.StoreName}"
        };
    }
}