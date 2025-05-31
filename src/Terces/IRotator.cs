namespace Terces;

/// <summary>
/// Represents a rotator interface that defines methods to initialize and execute
/// secret rotation strategies for a resource. Implementations of this interface
/// perform specific rotation logic depending on the resource type and strategy.
/// </summary>
public interface IRotator
{
    /// Gets a string identifier representing the type of rotation strategy implemented by the rotator.
    /// This property is static and specific to the implementing class, allowing different rotators
    /// to define distinct strategy types for categorization or identification purposes.
    /// Typically used to match resources or operations to the appropriate rotator implementation.
    static abstract string StrategyType { get; }

    /// <summary>
    /// Initializes the rotation process for a given resource using the provided configuration,
    /// secret store, and operation context.
    /// </summary>
    /// <param name="resource">The configuration details of the resource to initialize.</param>
    /// <param name="store">The secret store that manages the associated secrets of the resource.</param>
    /// <param name="context">The context of the operation, including credentials and other parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a
    /// <see cref="RotationResult"/> indicating the status of the initialization process.</returns>
    Task<RotationResult> InitializeAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Rotates the specified resource using the configured rotation strategy and updates its secret if needed.
    /// </summary>
    /// <param name="resource">The resource configuration, specifying details about the target resource for rotation.</param>
    /// <param name="store">The secret store for accessing and managing the resource's secrets.</param>
    /// <param name="context">The operation context, containing additional details and options for processing the rotation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the rotation process.</returns>
    Task<RotationResult> RotateAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);
}