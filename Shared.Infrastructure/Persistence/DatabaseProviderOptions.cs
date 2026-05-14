using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Persistence;

public enum DatabaseProvider
{
    SqlServer,
    Sqlite,
}

public static class DatabaseProviderResolver
{
    public const string ProviderConfigKey = "DefaultDatabaseProvider";

    public const string SqlServerConnectionStringName = "DefaultConnection";
    public const string SqliteAppConnectionStringName = "SqliteAppConnection";
    public const string SqliteAuthConnectionStringName = "SqliteAuthConnection";

    public static DatabaseProvider Resolve(IConfiguration configuration)
    {
        var value = configuration[ProviderConfigKey];
        return string.Equals(value, "Sqlite", System.StringComparison.OrdinalIgnoreCase)
            ? DatabaseProvider.Sqlite
            : DatabaseProvider.SqlServer;
    }

    public static string GetConnectionString(IConfiguration configuration, DatabaseProvider provider, bool isAuth)
    {
        if (provider == DatabaseProvider.SqlServer)
        {
            var conn = configuration.GetConnectionString(SqlServerConnectionStringName)
                       ?? configuration[SqlServerConnectionStringName]
                       ?? throw new System.InvalidOperationException(
                           $"Missing connection string '{SqlServerConnectionStringName}'.");
            return conn;
        }

        var key = isAuth ? SqliteAuthConnectionStringName : SqliteAppConnectionStringName;
        var raw = configuration.GetConnectionString(key)
                  ?? throw new System.InvalidOperationException(
                      $"Missing connection string '{key}' for SQLite provider.");
        return System.Environment.ExpandEnvironmentVariables(raw);
    }
}
