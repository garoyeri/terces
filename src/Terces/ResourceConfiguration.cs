namespace Terces;

/// <summary>
/// Represents the configuration settings for a resource, including details about its name, strategy, storage location,
/// expiration policies, and content type.
/// </summary>
public record ResourceConfiguration
{
    /// <summary>
    /// Gets the name of the resource.
    /// </summary>
    /// <remarks>
    /// Represents a unique identifier or the name associated with a resource in the configuration.
    /// Used in operations such as secret rotation to specify the target resource.
    /// </remarks>
    public required string Name { get; init; }

    /// <summary>
    /// Represents the type of strategy to be used for configuring or managing a resource.
    /// This property is required and determines the specific approach or methodology
    /// applied when interacting with the associated resource, such as secret rotation.
    /// </summary>
    public required string StrategyType { get; init; }

    /// Gets or sets the name of the secret store where the resource is stored or managed.
    /// This property is used to specify the target store for operations such as
    /// secret rotation, initialization, or retrieval.
    public required string StoreName { get; init; }

    /// <summary>
    /// Specifies the number of days until the resource's expiration.
    /// This property is used to define the interval at which the expiration time
    /// is updated during operations like secret rotation or resource management.
    /// </summary>
    public double ExpirationDays { get; set; } = 90.0;

    /// <summary>
    /// Specifies the number of days before a secret's actual expiration date during which
    /// the secret is eligible for rotation. This overlap period allows proactive rotation
    /// to ensure continuity and reduce downtime due to expired secrets.
    /// </summary>
    /// <remarks>
    /// The value represents an added buffer towards the expiration date during which
    /// the rotation process can be triggered.
    /// </remarks>
    /// <value>
    /// A double representing days of overlap; defaults to 0.0.
    /// </value>
    public double ExpirationOverlapDays { get; set; } = 0.0;

    /// <summary>
    /// Represents the content type associated with a resource.
    /// This property specifies the MIME type that defines the nature and format of the data stored in the resource.
    /// It is used in various operations to determine how the data should be processed or interpreted.
    /// The default value is "text/plain".
    /// </summary>
    public string ContentType { get; set; } = "text/plain";

    /// <summary>
    /// Gets or sets the unique identifier of the target resource to be managed or rotated.
    /// </summary>
    /// <remarks>
    /// This property is used to specify the resource that the rotation operation is targeting.
    /// If not set, the rotation operation cannot proceed.
    /// </remarks>
    public string? TargetResourceId { get; set; }
    
    /// <summary>
    /// Gets or sets the database user configuration if this resource is a database user resource.
    /// </summary>
    public DatabaseUserConfiguration? DatabaseUser { get; set; }
}