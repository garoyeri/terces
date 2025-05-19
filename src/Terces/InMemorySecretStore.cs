using System.Collections.Concurrent;

namespace Terces;

public class InMemorySecretStore : ISecretStore
{
    private const string StoreId = "in-memory-store";

    private record SecretEntry(SecretInfo Info, string SecretValue);

    readonly ConcurrentDictionary<string, SecretEntry> _secrets = new();
    
    public Task<SecretInfo?> GetSecretAsync(string name, CancellationToken cancellationToken)
    {
        var result = _secrets.GetValueOrDefault(name);
        return Task.FromResult(result?.Info);
    }

    public Task<SecretInfo?> UpdateSecretAsync(string name, string value, DateTimeOffset? expiresOn, string contentType,
        CancellationToken cancellationToken = default)
    {
        var result = _secrets.AddOrUpdate(name,
            s => new SecretEntry(
                new SecretInfo(name, name, contentType, true, DateTimeOffset.UtcNow, expiresOn, DateTimeOffset.UtcNow,
                    StoreId, "1"),
                value
            ),
            (s, entry) => new SecretEntry(
                new SecretInfo(name, name, contentType, entry.Info.Enabled, entry.Info.CreatedOn,
                    expiresOn, DateTimeOffset.UtcNow, entry.Info.StoreId, entry.Info.Version),
                value
            )
        );

        return Task.FromResult<SecretInfo?>(result.Info);
    }
}