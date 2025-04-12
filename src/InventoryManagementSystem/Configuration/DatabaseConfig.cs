using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Configuration;

/// <summary>
/// Configuration settings for the database connection
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// Gets or sets the database connection string
    /// </summary>
    [Required(ErrorMessage = "Connection string is required")]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command timeout in seconds (default: 60 seconds)
    /// </summary>
    [Range(30, 300, ErrorMessage = "Command timeout must be between 30 and 300 seconds")]
    public int CommandTimeout { get; set; } = 60;

    /// <summary>
    /// Gets or sets whether to enable detailed error messages (development only)
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// Gets or sets whether to enable sensitive data logging
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed connections
    /// </summary>
    [Range(1, 5, ErrorMessage = "Max retry count must be between 1 and 5")]
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum delay between retries in seconds
    /// </summary>
    [Range(1, 15, ErrorMessage = "Max retry delay must be between 1 and 15 seconds")]
    public int MaxRetryDelay { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether to enable automatic migration (development only)
    /// </summary>
    public bool EnableAutoMigration { get; set; }
} 