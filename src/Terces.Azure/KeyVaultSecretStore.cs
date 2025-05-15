using System.Net.Mime;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace Terces.Azure;

public class KeyVaultSecretStore : ISecretStore
{
    private readonly SecretClient _client;

    public KeyVaultSecretStore(Uri vaultUri, TokenCredential credential)
    {
        _client = new SecretClient(vaultUri, credential);
    }
    
    public async Task<SecretInfo?> GetSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        var secretFound = await _client.GetSecretAsync(name, null, cancellationToken);
        return secretFound.Map();
    }

    public async Task<SecretInfo?> UpdateSecretAsync(string name, string value, DateTimeOffset? expiresOn = null,
        string contentType = MediaTypeNames.Text.Plain, CancellationToken cancellationToken = default)
    {
        var secret = new KeyVaultSecret(name, value)
        {
            Properties =
            {
                ContentType = contentType,
                Enabled = true,
                ExpiresOn = expiresOn
            }
        };
        var secretUpdated = await _client.SetSecretAsync(secret, cancellationToken);
        return secretUpdated.Map();
    }
}