namespace Terces;

/// <summary>Represents the necessary credentials for accessing a database.</summary>
/// <remarks>
/// Provides a secure and structured way to store and transfer the database's hostname,
/// administrative username, and corresponding password during secret management operations.
/// Typically used in the process of credential rotation to ensure secure database access.
/// </remarks>
/// <param name="hostname">The hostname or IP address of the database server.</param>
/// <param name="username">The administrative username for the database.</param>
/// <param name="password">The password associated with the database user.</param>
public record DatabaseCredential(string hostname, string username, string password);