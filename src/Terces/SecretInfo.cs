namespace Terces;

/// <summary>
/// Represents metadata information about a secret.
/// </summary>
/// <remarks>
/// This class encapsulates details about a secret, including its unique identifier, name, content type,
/// creation and expiration details, and associated store information.
/// </remarks>
/// <param name="Id">Identifier for the secret, assigned by the secret store.</param>
/// <param name="Name">Name for the secret, used to get the secret from the store.</param>
/// <param name="ContentType">Content-Type for the secret, usually `text/plain` or `application/json`.</param>
/// <param name="Enabled">Whether the secret is enabled in the secret store.</param>
/// <param name="CreatedOn">When the secret was created.</param>
/// <param name="ExpiresOn">When the secret will expire and should no longer be used.</param>
/// <param name="UpdatedOn">When the secret was last updated.</param>
/// <param name="StoreId">Identifier for the store.</param>
/// <param name="Version">Version of the secret, `null` refers to the latest version.</param>
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
