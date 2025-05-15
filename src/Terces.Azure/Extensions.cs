using Azure;
using Azure.Security.KeyVault.Secrets;

namespace Terces.Azure;

public static class Extensions
{
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