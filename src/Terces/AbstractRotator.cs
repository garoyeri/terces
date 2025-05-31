namespace Terces;

/// <summary>
/// Abstract base class that provides a foundational structure for configuring,
/// initializing, and rotating resources or secrets. Implements mechanisms
/// to evaluate rotation candidacy and delegates the specific details of initialization
/// and rotation processes to subclasses.
/// </summary>
public abstract class AbstractRotator
{
    /// <summary>
    /// Represents the time provider used for evaluating initialization and rotation candidacy,
    /// as well as determining expiration details within the rotator logic.
    /// </summary>
    /// <remarks>
    /// This variable is instantiated through the constructor of the <see cref="AbstractRotator"/> class.
    /// It enables time-related functionalities, such as getting the current UTC time for computation purposes.
    /// </remarks>
    protected readonly TimeProvider _time;

    /// <summary>
    /// Provides a base class to facilitate the configuration, initialization, and rotation
    /// of resources or secrets. This abstract class defines mechanisms to evaluate whether
    /// a resource requires initialization or rotation while delegating the implementation
    /// details of the specific operations to its derived classes.
    /// </summary>
    protected AbstractRotator(TimeProvider time)
    {
        _time = time;
    }


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

    /// <summary>
    /// Performs the initialization process for secret rotation.
    /// This is an abstract method that must be implemented by derived classes
    /// to define the specific initialization logic.
    /// </summary>
    /// <param name="resource">The resource configuration associated with the secret rotation process.</param>
    /// <param name="store">The secret store used to store or retrieve secrets associated with the resource.</param>
    /// <param name="context">The operational context providing additional information for the rotation process.</param>
    /// <param name="cancellationToken">A token to signal the cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the rotation result.</returns>
    protected abstract Task<RotationResult> PerformInitialization(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);

    /// Performs the rotation of a given resource configuration.
    /// <param name="resource">
    /// The resource configuration that specifies the details of the resource to be rotated.
    /// </param>
    /// <param name="store">
    /// The secret store that will be used to perform operations related to secrets.
    /// </param>
    /// <param name="context">
    /// The operation context that provides additional information or state for the rotation process.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the rotation operation to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// <see cref="RotationResult"/> which indicates the outcome of the rotation.
    /// </returns>
    protected abstract Task<RotationResult> PerformRotation(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);
   
}