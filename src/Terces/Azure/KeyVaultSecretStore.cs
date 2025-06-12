using System.Net.Mime;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace Terces.Azure;

/// <summary>
/// Provides an Azure Key Vault implementation of <see cref="ISecretStore"/>.
/// </summary>
public class KeyVaultSecretStore : ISecretStore
{
    /// <summary>
    /// The Azure Key Vault client used to interact with the vault.
    /// </summary>
    private readonly SecretClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyVaultSecretStore"/> class.
    /// </summary>
    /// <param name="vaultUri">The URI of the Key Vault.</param>
    /// <param name="credential">The credential used to authenticate with Azure Key Vault.</param>
    public KeyVaultSecretStore(Uri vaultUri, TokenCredential credential)
    {
        _client = new SecretClient(vaultUri, credential);
    }
    
    /// <summary>
    /// Retrieves the metadata for a secret by its name from Azure Key Vault.
    /// </summary>
    /// <param name="name">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The secret's metadata if found; otherwise, null.</returns>
    public async Task<SecretInfo?> GetSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        var secretFound = await _client.GetSecretAsync(name, null, cancellationToken);
        return secretFound.Map();
    }

    /// <summary>
    /// Creates or updates a secret in Azure Key Vault with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="value">The value of the secret.</param>
    /// <param name="expiresOn">The expiration date of the secret, if any.</param>
    /// <param name="contentType">The content type of the secret.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The metadata of the created or updated secret.</returns>
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

    /// <summary>
    /// Retrieves the value of a secret stored in the Azure Key Vault.
    /// </summary>
    /// <param name="name">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The value of the secret if found; otherwise, null.</returns>
    public async Task<string?> GetSecretValueAsync(string name, CancellationToken cancellationToken)
    {
        var secretValueFound = await _client.GetSecretAsync(name, null, cancellationToken);
        if (!secretValueFound.HasValue) return null;

        return secretValueFound.Value.Value;
    }
}