namespace Terces;

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