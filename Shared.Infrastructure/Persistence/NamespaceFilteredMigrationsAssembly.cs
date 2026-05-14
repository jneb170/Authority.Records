using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

// MigrationsAssembly is marked internal but is the documented extension point
// for customizing migration discovery. Acceptable trade-off for the provider toggle.
#pragma warning disable EF1001

namespace Shared.Infrastructure.Persistence;

internal abstract class NamespaceFilteredMigrationsAssembly : MigrationsAssembly
{
    private readonly string _allowedNamespaceSuffix;
    private readonly System.Type _contextType;
    private ModelSnapshot? _modelSnapshot;
    private bool _snapshotResolved;

    protected NamespaceFilteredMigrationsAssembly(
        ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
        string allowedNamespaceSuffix)
        : base(currentContext, options, idGenerator, logger)
    {
        _allowedNamespaceSuffix = allowedNamespaceSuffix;
        _contextType = currentContext.Context.GetType();
    }

    public override IReadOnlyDictionary<string, TypeInfo> Migrations =>
        base.Migrations
            .Where(kvp => kvp.Value.Namespace?.EndsWith(_allowedNamespaceSuffix, System.StringComparison.Ordinal) == true)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public override ModelSnapshot? ModelSnapshot
    {
        get
        {
            if (_snapshotResolved)
                return _modelSnapshot;

            var snapshotType = Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ModelSnapshot)))
                .Where(t => t.Namespace?.EndsWith(_allowedNamespaceSuffix, System.StringComparison.Ordinal) == true)
                .Where(t => t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType)
                .FirstOrDefault();

            _modelSnapshot = snapshotType is null
                ? null
                : (ModelSnapshot?)System.Activator.CreateInstance(snapshotType);
            _snapshotResolved = true;
            return _modelSnapshot;
        }
    }
}

internal sealed class SqlServerMigrationsAssembly : NamespaceFilteredMigrationsAssembly
{
    public SqlServerMigrationsAssembly(
        ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        : base(currentContext, options, idGenerator, logger, ".SqlServer")
    {
    }
}

internal sealed class SqliteMigrationsAssembly : NamespaceFilteredMigrationsAssembly
{
    public SqliteMigrationsAssembly(
        ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        : base(currentContext, options, idGenerator, logger, ".Sqlite")
    {
    }
}
