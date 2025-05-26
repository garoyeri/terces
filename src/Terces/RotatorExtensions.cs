namespace Terces;

public static class RotatorExtensions
{
    public static bool ShouldRotate(this SecretInfo info,
        DateTimeOffset now,
        double? expirationOverlapDays = 0.0)
    {
        if (!info.ExpiresOn.HasValue) return false;
        expirationOverlapDays ??= 0.0;
        var daysToExpire = (info.ExpiresOn.Value - now).TotalDays;
        
        return daysToExpire <= expirationOverlapDays;
    }

    public static async Task<RotationResult?> EvaluateRotationCandidacy(this ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        TimeProvider time,
        CancellationToken cancellationToken = default)
    {
        var secret = await store.GetSecretAsync(resource.Name, cancellationToken);
        if (secret == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Secret {resource.Name} not found in store {resource.StoreName}"
            };
        }

        if (!context.Force && !secret.ShouldRotate(time.GetUtcNow(), resource.ExpirationOverlapDays))
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Secret {resource.Name} is not due for rotation"
            };
        }

        return null;
    }

    public static async Task<RotationResult?> EvaluateInitializationCandidacy(this ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        TimeProvider time,
        CancellationToken cancellationToken = default)
    {
        var secret = await store.GetSecretAsync(resource.Name, cancellationToken);
        if (secret != null && !context.Force)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Secret {resource.Name} already initialized in store {resource.StoreName}"
            };
        }

        return null;
    }
}