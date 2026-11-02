using Modules.Records.Domain.DomainEvents;
using System.Collections.Concurrent;
using System.Reflection;

namespace Shared.Infrastructure.Outbox;

/// <summary>
/// Maps the type names stored in the outbox back to domain event <see cref="Type"/>s so messages can be deserialized.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Assembly.GetTypes"/> only returns non-generic types and <em>open</em> generic definitions
/// (for example <c>LockAcquiredDomainEvent`1</c>), but the outbox stores the <em>closed</em> type name
/// (<c>LockAcquiredDomainEvent`1[[...Incident...]]</c>). Closed generic names are therefore resolved on demand
/// and cached.
/// </para>
/// <para>
/// Resolution is restricted to the trusted assemblies the registry was created with: assembly references in the
/// stored name are matched by simple name against that list (anything else fails to resolve), the generic definition
/// must be one of the registered open event types, and the result must be a concrete <see cref="IDomainEvent"/>.
/// </para>
/// </remarks>
public sealed class DomainEventTypeRegistry
{
    private readonly Dictionary<string, Type> _types;
    private readonly HashSet<Type> _openGenericDefinitions;
    private readonly Dictionary<string, Assembly> _trustedAssemblies;
    private readonly ConcurrentDictionary<string, Type?> _resolved = new(StringComparer.Ordinal);

    public DomainEventTypeRegistry(params Assembly[] assemblies)
    {
        var eventTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IDomainEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                t.IsClass)
            .ToList();

        _types = eventTypes
            .Where(t => !t.ContainsGenericParameters && t.AssemblyQualifiedName != null)
            .ToDictionary(t => t.AssemblyQualifiedName!, t => t);

        _openGenericDefinitions = eventTypes
            .Where(t => t.IsGenericTypeDefinition)
            .ToHashSet();

        // Generic arguments (e.g. Incident) live in the same trusted assemblies; core types (Guid, etc.) in CoreLib.
        _trustedAssemblies = assemblies
            .Append(typeof(object).Assembly)
            .Distinct()
            .ToDictionary(a => a.GetName().Name!, a => a, StringComparer.Ordinal);
    }

    /// <summary>
    /// Attempts to retrieve a domain event type by its assembly-qualified name.
    /// </summary>
    /// <param name="typeName">The assembly-qualified name stored in <see cref="OutboxMessage.Type"/>.</param>
    /// <param name="type">The resolved type, or null when the name is unknown or not a trusted domain event.</param>
    /// <returns><c>true</c> if the type was resolved; otherwise, <c>false</c>.</returns>
    public bool TryGet(string typeName, out Type? type)
    {
        if (_types.TryGetValue(typeName, out type))
            return true;

        type = _resolved.GetOrAdd(typeName, ResolveClosedGeneric);
        return type is not null;
    }

    private Type? ResolveClosedGeneric(string typeName)
    {
        Type? candidate;
        try
        {
            candidate = Type.GetType(
                typeName,
                assemblyResolver: name => name.Name is { } simpleName && _trustedAssemblies.TryGetValue(simpleName, out var asm)
                    ? asm
                    : null,
                typeResolver: (asm, name, ignoreCase) => asm?.GetType(name, throwOnError: false, ignoreCase),
                throwOnError: false);
        }
        catch (Exception ex) when (ex is ArgumentException or TypeLoadException or FileLoadException or BadImageFormatException)
        {
            return null;
        }

        if (candidate is null ||
            !candidate.IsConstructedGenericType ||
            !_openGenericDefinitions.Contains(candidate.GetGenericTypeDefinition()) ||
            !typeof(IDomainEvent).IsAssignableFrom(candidate) ||
            candidate.IsAbstract)
        {
            return null;
        }

        return candidate;
    }
}
