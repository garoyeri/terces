namespace Terces;

public interface IRotator
{
    static abstract string StrategyType { get; }
    
    Task<RotationResult> InitializeAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);
    Task<RotationResult> RotateAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken);
}