using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

namespace ArlaNatureConnect.WinUI.ViewModels;

/// <summary>
/// ViewModel for a dialog that helps users construct a database connection string.
/// </summary>
public sealed partial class ConnectionDialogViewModel : ViewModelBase
{
    #region Fields
    /// <summary>
    /// Flag to suppress updates to <see cref="ConnectionString"/> while parsing or resetting fields.
    /// </summary>
    private bool _suppressUpdate;

    /// <summary>
    /// Backing field for the <see cref="ConnectionString"/> property.
    /// </summary>
    private string? _connectionString;

    /// <summary>
    /// Backing field for the <see cref="ServerName"/> property.
    /// </summary>
    private string? _serverName;

    /// <summary>
    /// Backing field for the <see cref="IntegratedSecurity"/> property. Defaults to <c>true</c>.
    /// </summary>
    private bool _integratedSecurity = true;

    /// <summary>
    /// Backing field for the <see cref="UserName"/> property.
    /// </summary>
    private string? _userName;

    /// <summary>
    /// Backing field for the <see cref="Password"/> property.
    /// </summary>
    private string? _password;

    /// <summary>
    /// Backing field for the <see cref="DatabaseName"/> property.
    /// </summary>
    private string? _databaseName;

    /// <summary>
    /// Backing field for the <see cref="Encrypt"/> property.
    /// </summary>
    private bool _encrypt;

    /// <summary>
    /// Backing field for the <see cref="TrustServerCertificate"/> property.
    /// </summary>
    private bool _trustServerCertificate;
    #endregion
    #region Fields Commands
    #endregion
    #region Event handlers
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionDialogViewModel"/> class and
    /// resets fields to their default values.
    /// </summary>
    public ConnectionDialogViewModel()
    {
        ResetFields(true);
    }

    #region Properties
    #endregion
    #region Observables Properties
    /// <summary>
    /// Gets or sets the full connection string composed from individual fields.
    /// When set, the connection string is parsed into individual fields unless updates are suppressed.
    /// </summary>
    public string? ConnectionString
    {
        get => _connectionString;
        set
        {
            if (_connectionString == value) return;
            _connectionString = value;
            OnPropertyChanged();
            if (!_suppressUpdate)
            {
                ParseConnectionString(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the server/data source name.
    /// </summary>
    public string? ServerName
    {
        get => _serverName;
        set
        {
            if (_serverName == value) return;
            _serverName = value;
            OnPropertyChanged();
            UpdateConnectionString();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether integrated security (Windows authentication) should be used.
    /// </summary>
    public bool IntegratedSecurity
    {
        get => _integratedSecurity;
        set
        {
            if (_integratedSecurity == value) return;
            _integratedSecurity = value;
            OnPropertyChanged();
            UpdateConnectionString();
        }
    }

    /// <summary>
    /// Gets or sets the SQL user name (used when <see cref="IntegratedSecurity"/> is <c>false</c>).
    /// </summary>
    public string? UserName
    {
        get => _userName;
        set
        {
            if (_userName == value) return;
            _userName = value;
            OnPropertyChanged();
            UpdateConnectionString();
        }
    }

    /// <summary>
    /// Gets or sets the SQL password (used when <see cref="IntegratedSecurity"/> is <c>false</c>).
    /// </summary>
    public string? Password
    {
        get => _password;
        set
        {
            if (_password == value) return;
            _password = value;
            OnPropertyChanged();
            UpdateConnectionString();
        }
    }

    /// <summary>
    /// Gets or sets the database/initial catalog name.
    /// </summary>
    public string? DatabaseName
    {
        get => _databaseName;
        set
        {
            if (_databaseName == value) return;
            _databaseName = value;
            OnPropertyChanged();
            UpdateConnectionString();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to enable encryption for the connection.
    /// </summary>
    public bool Encrypt
    {
        get => _encrypt;
        set
        {
            if (_encrypt == value) return;
            _encrypt = value;
            OnPropertyChanged();
            UpdateConnectionString();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to trust the server certificate.
    /// </summary>
    public bool TrustServerCertificate
    {
        get => _trustServerCertificate;
        set
        {
            if (_trustServerCertificate == value) return;
            _trustServerCertificate = value;
            OnPropertyChanged();
            UpdateConnectionString();
        }
    }
    #endregion
    #region Load handler
    #endregion
    #region Commands
    #endregion
    #region CanXXX Command
    #endregion
    #region OnXXX Command
    #endregion
    #region Helpers
    /// <summary>
    /// Reset all input fields to their default values. Optionally recomposes the connection string from fields.
    /// </summary>
    /// <param name="updateConnectionString">If <c>true</c>, the connection string will be recomposed after reset.</param>
    public void ResetFields(bool updateConnectionString = false)
    {
        _suppressUpdate = true;

        // Set backing fields directly to avoid triggering UpdateConnectionString repeatedly
        _serverName = null; OnPropertyChanged(nameof(ServerName));
        _databaseName = null; OnPropertyChanged(nameof(DatabaseName));
        _integratedSecurity = true; OnPropertyChanged(nameof(IntegratedSecurity));
        _userName = null; OnPropertyChanged(nameof(UserName));
        _password = null; OnPropertyChanged(nameof(Password));
        _encrypt = false; OnPropertyChanged(nameof(Encrypt));
        _trustServerCertificate = false; OnPropertyChanged(nameof(TrustServerCertificate));

        _suppressUpdate = false;

        if (updateConnectionString)
        {
            UpdateConnectionString();
        }
    }


    /// <summary>
    /// Build the connection string from the current per-field values and assign it to <see cref="ConnectionString"/>.
    /// </summary>
    private void UpdateConnectionString()
    {
        try
        {
            _suppressUpdate = true;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(ServerName))
            {
                parts.Add($"Server={ServerName}");
            }

            if (!string.IsNullOrWhiteSpace(DatabaseName))
            {
                parts.Add($"Database={DatabaseName}");
            }

            if (IntegratedSecurity)
            {
                parts.Add("Integrated Security=True");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(UserName)) parts.Add($"User ID={UserName}");
                if (!string.IsNullOrWhiteSpace(Password)) parts.Add($"Password={Password}");
            }

            parts.Add($"Encrypt={(Encrypt ? "True" : "False")}");
            parts.Add($"TrustServerCertificate={(TrustServerCertificate ? "True" : "False")}");

            ConnectionString = string.Join(";", parts) + ";";
        }
        finally
        {
            _suppressUpdate = false;
        }
    }

    /// <summary>
    /// Parse a connection string and populate the individual fields. Respects the suppression flag to avoid recursive updates.
    /// </summary>
    /// <param name="connectionString">The connection string to parse. May be <c>null</c> or empty.</param>
    private void ParseConnectionString(string? connectionString)
    {
        _suppressUpdate = true;
        try
        {
            // Reset
            ResetFields(updateConnectionString: false);

            if (string.IsNullOrWhiteSpace(connectionString)) return;

            var entries = connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p =>
                {
                    int idx = p.IndexOf('=');
                    if (idx < 0) return new KeyValuePair<string, string>(p.Trim(), string.Empty);
                    string key = p[..idx].Trim();
                    string val = p[(idx + 1)..].Trim();
                    return new KeyValuePair<string, string>(key, val);
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            if (entries.TryGetValue("Server", out string? server) || entries.TryGetValue("Data Source", out server))
            {
                ServerName = server;
            }

            if (entries.TryGetValue("Database", out var database) || entries.TryGetValue("Initial Catalog", out database))
            {
                DatabaseName = database;
            }

            if (entries.TryGetValue("Integrated Security", out var integ) || entries.TryGetValue("Trusted_Connection", out integ))
            {
                IntegratedSecurity = string.Equals(integ, "True", StringComparison.OrdinalIgnoreCase) || string.Equals(integ, "SSPI", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                IntegratedSecurity = false;
            }

            if (entries.TryGetValue("User ID", out var user))
            {
                UserName = user;
            }

            if (entries.TryGetValue("Password", out var pwd))
            {
                Password = pwd;
            }

            if (entries.TryGetValue("Encrypt", out var enc))
            {
                Encrypt = string.Equals(enc, "True", StringComparison.OrdinalIgnoreCase);
            }

            if (entries.TryGetValue("TrustServerCertificate", out var tsc))
            {
                TrustServerCertificate = string.Equals(tsc, "True", StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            _suppressUpdate = false;
        }
    }
    #endregion
}
