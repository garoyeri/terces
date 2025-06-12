using Azure;
using Azure.Security.KeyVault.Secrets;

namespace Terces.Azure;

/// <summary>
/// Provides extension methods for working with Azure KeyVault secrets or related Azure SDK constructs.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Converts a <see cref="Response{KeyVaultSecret}"/> object to a <see cref="SecretInfo"/> object.
    /// </summary>
    /// <param name="response">The response containing the KeyVaultSecret to convert.</param>
    /// <returns>A <see cref="SecretInfo"/> object if the response has a valid secret; otherwise, null.</returns>
    internal static SecretInfo? Map(this Response<KeyVaultSecret> response)
    {
        if (!response.HasValue) return null;
        return new SecretInfo(
            response.Value.Id.ToString(),
            response.Value.Name,
            response.Value.Properties.ContentType,
            response.Value.Properties.Enabled ?? true,
            response.Value.Properties.CreatedOn ?? DateTimeOffset.UnixEpoch,
            response.Value.Properties.ExpiresOn,
            response.Value.Properties.UpdatedOn ?? DateTimeOffset.UnixEpoch,
            response.Value.Properties.VaultUri.ToString(),
            response.Value.Properties.Version
        );
    }
}