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
}