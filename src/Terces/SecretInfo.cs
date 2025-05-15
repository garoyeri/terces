namespace Terces;

public record SecretInfo(
    string Id,
    string Name,
    string ContentType,
    bool Enabled,
    DateTimeOffset CreatedOn,
    DateTimeOffset? ExpiresOn,
    DateTimeOffset UpdatedOn,
    string StoreId,
    string? Version);
