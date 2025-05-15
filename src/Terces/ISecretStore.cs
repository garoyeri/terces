namespace Terces;

public interface ISecretStore
{
    Task<SecretInfo?> GetSecretAsync(string name, CancellationToken cancellationToken);
    Task<SecretInfo?> UpdateSecretAsync(string name, string value, DateTimeOffset? expiresOn, string contentType,
        CancellationToken cancellationToken);
}