namespace Terces;

/// <summary>
/// Abstract base class that provides a foundational structure for configuring,
/// initializing, and rotating resources or secrets. Implements mechanisms
/// to evaluate rotation candidacy and delegates the specific details of initialization
/// and rotation processes to subclasses.
/// </summary>
public abstract class AbstractRotator
{
    protected readonly TimeProvider _time;

    protected AbstractRotator(TimeProvider time)
    {
        _time = time;
    }


    public async Task<RotationResult> InitializeAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        var initialResult = await resource.EvaluateInitializationCandidacy(store, context, _time, cancellationToken);
        if (initialResult != null) return initialResult;
        
        return await PerformInitialization(resource, store, context, cancellationToken);
    }

    public async Task<RotationResult> RotateAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        var initialResult = await resource.EvaluateRotationCandidacy(store, context, _time, cancellationToken);
        if (initialResult != null) return initialResult;
        
        return await PerformRotation(resource, store, context, cancellationToken);
    }

    protected abstract Task<RotationResult> PerformInitialization(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);

    protected abstract Task<RotationResult> PerformRotation(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);
   
}