namespace Terces;

/// <summary>
/// Represents the configuration settings for a database user.
/// </summary>
public record DatabaseUserConfiguration
{
    /// Gets or sets the prefix to be used for usernames in the database configuration.
    /// This property defines a string prepended to usernames, typically used
    /// to create standardized naming conventions or identifiers.
    public string NamePrefix { get; set; } = "u";

    /// <summary>
    /// Gets or sets the roles associated with the database user.
    /// </summary>
    public string[] Roles { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the name of the secret used for the database server credential.
    /// This credential should be a secret of content type `application/json` with properties:
    /// `hostname`, `username`, and `password`.
    /// </summary>
    public required string ServerSecretName { get; init; }
    
    /// <summary>
    /// Gets or sets the endpoint to use for connecting to the database.
    /// </summary>
    public required string Hostname { get; init; }
}