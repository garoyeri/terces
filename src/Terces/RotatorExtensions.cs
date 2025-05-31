namespace Terces;

/// <summary>
/// Provides extension methods for handling secret rotation and initialization operations.
/// </summary>
public static class RotatorExtensions
{
    /// <summary>
    /// Determines whether a secret should be rotated based on its expiration information.
    /// </summary>
    /// <param name="info">The secret information containing expiration details.</param>
    /// <param name="now">The current date and time used to evaluate rotation necessity.</param>
    /// <param name="expirationOverlapDays">
    /// An optional number of days used to define an overlap period before expiration.
    /// Defaults to 0.0 if not specified.
    /// </param>
    /// <returns>
    /// True if the secret should be rotated due to being close to or past its expiration date; otherwise, false.
    /// </returns>
    public static bool ShouldRotate(this SecretInfo info,
        DateTimeOffset now,
        double? expirationOverlapDays = 0.0)
    {
        if (!info.ExpiresOn.HasValue) return false;
        expirationOverlapDays ??= 0.0;
        var daysToExpire = (info.ExpiresOn.Value - now).TotalDays;
        
        return daysToExpire <= expirationOverlapDays;
    }

    /// <summary>
    /// Evaluates if a resource's secret is a candidate for rotation, based on its current state, expiration settings,
    /// and the operation context. If rotation is not required or forced, a <see cref="RotationResult"/> is returned
    /// with relevant details; otherwise, returns null to indicate it is eligible for rotation.
    /// </summary>
    /// <param name="resource">
    /// The configuration of the resource whose secret is being evaluated for rotation.
    /// </param>
    /// <param name="store">
    /// The secret store that contains the resource's associated secret data.
    /// </param>
    /// <param name="context">
    /// Provides the operational context, including whether the evaluation forces rotation regardless of expiration
    /// criteria.
    /// </param>
    /// <param name="time">
    /// Provides the current time, used to assess the expiration or overlap conditions of the secret.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="RotationResult"/> indicating the outcome of the evaluation if rotation is not required or
    /// forced, or null if the secret is eligible for rotation.
    /// </returns>
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

    /// <summary>
    /// Asynchronously evaluates whether a secret initialization operation is necessary for the given resource.
    /// </summary>
    /// <param name="resource">The resource configuration containing metadata for the secret to be initialized.</param>
    /// <param name="store">The secret store where the secret will be checked and potentially initialized.</param>
    /// <param name="context">The operation context, which may include flags such as whether to force initialization.</param>
    /// <param name="time">A provider for the current date and time.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="RotationResult"/> containing information about the initialization attempt
    /// if the secret is already initialized, or null if initialization is required.
    /// </returns>
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