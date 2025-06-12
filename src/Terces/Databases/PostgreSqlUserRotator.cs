using System.Text.Json;
using Npgsql;
using Terces.Azure;

namespace Terces.Databases;

/// <summary>
/// Represents a rotator responsible for managing PostgreSQL user rotations.
/// </summary>
/// <remarks>
/// This class provides mechanisms to rotate PostgreSQL database user credentials. It inherits functionality
/// from AbstractRotator and implements the IRotator interface.
/// </remarks>
public class PostgreSqlUserRotator : AbstractRotator, IRotator
{
    /// <summary>
    /// Provides functionality for initializing and rotating PostgreSQL user credentials
    /// within a resource configuration. This class is a concrete implementation of
    /// <see cref="AbstractRotator"/> and conforms to the <see cref="IRotator"/> interface.
    /// </summary>
    public PostgreSqlUserRotator(TimeProvider time) : base(time)
    {
    }

    /// <summary>
    /// Represents the strategy type identifier for this rotator implementation.
    /// This value is used to distinguish the specific strategy being employed
    /// by the PostgreSqlUserRotator for handling database user rotation logic.
    /// Typically associated with "database/postgresql/user".
    /// </summary>
    public static string StrategyType => "database/postgresql/user";

    /// <summary>
    /// Performs the initialization process for the PostgreSQL user rotator, setting up
    /// the necessary resources and dependencies required for the rotation operation.
    /// </summary>
    /// <param name="resource">The resource configuration containing details about the resource to be rotated.</param>
    /// <param name="store">The secret store interface used to retrieve or update secrets during initialization.</param>
    /// <param name="context">The operation context containing additional metadata or parameters for the operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the initialization process.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="RotationResult"/> containing the result of the initialization.</returns>
    protected override Task<RotationResult> PerformInitialization(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        return PerformRotation(resource, store, context, cancellationToken);
    }

    /// <summary>
    /// Performs the rotation of a PostgreSQL database user, creating a new user with roles and updating the secret store.
    /// This method ensures that database configuration and role names are valid, validates server credentials,
    /// connects to the database, creates a new user with appropriate roles and expiration, and updates the
    /// secret store with the new user credentials.
    /// </summary>
    /// <param name="resource">
    /// Represents the resource configuration containing details for the rotation process, such as database user settings
    /// and expiration periods.
    /// </param>
    /// <param name="store">
    /// The secret store where credentials are retrieved and updated.
    /// </param>
    /// <param name="context">
    /// Provides context about the operation, including whether it is a dry-run (What-If) operation.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A <see cref="RotationResult"/> containing the results of the rotation process, including the status and any relevant notes.
    /// </returns>
    protected override async Task<RotationResult> PerformRotation(ResourceConfiguration resource,
        ISecretStore store,
        OperationContext context,
        CancellationToken cancellationToken)
    {
        // database configuration is required
        if (resource.DatabaseUser == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Database user configuration is required for {resource.Name} rotation in store {resource.StoreName}"
            };
        }

        // ensure role names are valid identifiers
        if (resource.DatabaseUser.Roles.Any(r => !IsValidIdentifier(r)))
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Invalid role name(s) in {resource.Name} rotation in store {resource.StoreName}"
            };
        }
        
        // server secret should exist (in the same store)
        var serverCredentialResponse = await store.GetSecretValueAsync(
            resource.DatabaseUser.ServerSecretName, cancellationToken);
        if (serverCredentialResponse == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes =
                    $"Database server secret {resource.DatabaseUser.ServerSecretName} for {resource.Name} not found in store {resource.StoreName}"
            };
        }

        var serverCredentials =
            JsonSerializer.Deserialize<DatabaseCredential>(
                serverCredentialResponse);
        if (serverCredentials == null)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Database server secret {resource.DatabaseUser.ServerSecretName} for {resource.Name} is not a valid JSON object in store {resource.StoreName}"
            };
        }
        
        // connect and validate database connection
        var connectionString = new NpgsqlConnectionStringBuilder()
        {
            Host = resource.DatabaseUser.Hostname,
            Username = serverCredentials.username,
            Password = serverCredentials.password
        };
        
        await using var connection = new NpgsqlConnection(connectionString.ToString());
        await connection.OpenAsync(cancellationToken);
        
        if (context.IsWhatIf)
        {
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = true,
                Notes = $"Would have rotated secret {resource.Name} in store {resource.StoreName}"
            };
        }

        // create the new user with roles
        var username = PasswordGenerator.GenerateUsername(resource.DatabaseUser.NamePrefix, 16);
        var password = PasswordGenerator.Generate(24);
        var roles = string.Join(", ", resource.DatabaseUser.Roles.Select(r => Quote(r)));
        var expiration = _time.GetUtcNow().AddDays(resource.ExpirationDays);
        var sql = $"CREATE USER \"{username}\" PASSWORD '{password}' IN ROLE {roles} VALID UNTIL '{expiration:u}';";

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
        
        // update the secret store
        var newCredential = new DatabaseCredential(resource.DatabaseUser.Hostname, username, password);
        
        var json = JsonSerializer.Serialize(newCredential);
        var updatedSecret = await store.UpdateSecretAsync(resource.Name, json, expiration, "application/json", cancellationToken);
        if (updatedSecret == null)
        {
            // something has gone horribly wrong
            return new RotationResult
            {
                Name = resource.Name,
                WasRotated = false,
                Notes = $"Failed to update secret for {resource.Name} in store {resource.StoreName}. Reinitialization will be required to recover."
            };
        }

        return new RotationResult
        {
            Name = resource.Name,
            WasRotated = true,
            Notes = $"Rotated secret {resource.Name} in store {resource.StoreName}"
        };
    }

    /// <summary>
    /// Validates whether a given identifier is a valid PostgreSQL identifier based on PostgreSQL rules.
    /// A valid PostgreSQL identifier must:
    /// - Start with a letter or an underscore.
    /// - Contain only letters, digits, underscores, or dollar signs.
    /// - Be no longer than 63 characters.
    /// </summary>
    /// <param name="identifier">
    /// The identifier to validate.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the identifier is valid; otherwise, returns <c>false</c>.
    /// </returns>
    public static bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;
        
        // PostgreSQL identifier rules:
        // - Must start with letter or underscore
        // - Can contain letters, digits, underscores, dollar signs
        // - Maximum 63 characters
        return identifier.Length <= 63 &&
            char.IsLetter(identifier[0]) || identifier[0] == '_' &&
            identifier.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '$');
    }

    /// Quotes the given identifier with double quotes by default or single quotes if specified.
    /// <param name="identifier">
    /// The string to be quoted. Typically, this represents a database object name such as a column, table, or user.
    /// </param>
    /// <param name="single">
    /// A boolean value indicating whether the identifier should be quoted with single quotes instead of double quotes. Defaults to false.
    /// </param>
    /// <returns>
    /// A string representing the quoted identifier, enclosed in either double or single quotes based on the parameter value.
    /// </returns>
    public static string Quote(string identifier, bool single = false)
    {
        return single ? $"'{identifier}'" : $"\"{identifier}\"";
    }
}