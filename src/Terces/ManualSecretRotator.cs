namespace Terces;

/// <summary>
/// This rotator will rotate a secret only in the secret store but not
/// be linked to any other resource. This is useful if you don't have permission
/// to change the secret on the target resource, or you've already rotated it
/// and need the store to match.
/// </summary>
public class ManualSecretRotator : IRotator
{
    private readonly TimeProvider _time;

    public ManualSecretRotator(TimeProvider time)
    {
        _time = time;
    }

    public static string StrategyType => "manual/generic";
    
    public async Task<RotationResult> RotateAsync(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        var initialResult = await resource.EvaluateRotationCandidacy(store, context, _time, cancellationToken);
        if (initialResult != null) return initialResult;
        
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